using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Telegram.Bot.Types;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Tests.Extensions;
using YoutubeMusicBot.Tests.Stubs;
using YoutubeMusicBot.Wrappers.Interfaces;
using TagFile = TagLib.File;

namespace YoutubeMusicBot.Tests
{
	[Parallelizable]
	public class MessageHandlerTests
	{
		private Fixture _fixture = null!;
		private AutoMock _autoMock = null!;

		[SetUp]
		public void Setup()
		{
			Sequence.ContextMode = SequenceContextMode.Async;

			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_autoMock = AutoMock.GetStrict(
				builder =>
				{
					Program.ConfigureContainer(null, builder);

					builder.RegisterGeneric(typeof(LoggerStub<>))
						.As(typeof(ILogger<>));
					builder.RegisterMediatrDependenciesMocks();
					builder.RegisterOpenGenericMock(
						typeof(IOptionsMonitor<>),
						defaultValue: DefaultValue.Mock);
					builder.RegisterMock(
						new Mock<ITgClientWrapper>());
				});
		}

		[Test]
		[TestCase(
			"https://youtu.be/wuROIJ0tRPU",
			"Loading \"Гоня & Довгий Пес - Бронепоїзд\" started.")]
		public async Task ShouldSendLoadingStartedOnEcho(
			string url,
			string expectedMessage)
		{
			LogsHolder.Executions.Clear();
			var message = _fixture
				.Build<MessageContext>()
				.With(m => m.Text, url)
				.Create();

			var messageHandler = _autoMock.Create<MessageHandler>();

			await messageHandler.Handle(new MessageHandler.Request(message));

			LogsHolder.Executions.Should()
				.NotContain(e => e.LogLevel >= LogLevel.Error);
			_autoMock.Mock<ITgClientWrapper>()
				.Verify(
					c => c.SendMessageAsync(
						expectedMessage,
						It.IsAny<CancellationToken>()),
					Times.Once);
		}

		[Test]
		[Timeout(120_000)] // 2 minutes
		[TestCaseSource(nameof(TestCases))]
		public async Task ShouldUploadAudioOnEcho(
			string url,
			IReadOnlyCollection<ExpectedFile> expectedFiles)
		{
			LogsHolder.Executions.Clear();
			var message = _fixture
				.Build<MessageContext>()
				.With(m => m.Text, url)
				.Create();
			var cacheFolder = _autoMock.Container
				.BeginMessageLifetimeScope(message)
				.Resolve<ICacheFolder>()
				.Value;
			var resultTagFiles = new List<TagFile>();
			_autoMock.Mock<ITgClientWrapper>()
				.Setup(
					m => m.SendAudioAsync(
						It.IsAny<FileInfo>(),
						It.IsAny<CancellationToken>()))
				.Callback<FileInfo, CancellationToken>(
					(audio, _) => resultTagFiles.Add(
						TagFile.Create(
							Path.Join(cacheFolder, audio.Name))))
				.ReturnsAsync(new Message());
			var messageHandler = _autoMock.Create<MessageHandler>();

			await messageHandler.Handle(new MessageHandler.Request(message));

			LogsHolder.Executions.Should()
				.NotContain(e => e.LogLevel >= LogLevel.Error);
			resultTagFiles.Should().HaveSameCount(expectedFiles);
			foreach (var (expected, result) in expectedFiles
				.Zip(resultTagFiles))
			{
				result.Tag.Title.Should().Be(expected.Title);
				result.Tag.FirstPerformer.Should().Be(expected.Author);
				result.Properties.Duration.Should()
					.BeCloseTo(
						expected.Duration,
						precision: TimeSpan.FromSeconds(1));
			}

			Directory.EnumerateFileSystemEntries(cacheFolder)
				.Should()
				.BeEmpty();
		}

		public static IEnumerable<TestCaseData> TestCases()
		{
			yield return new TestCaseData(
				"https://youtu.be/wuROIJ0tRPU",
				ImmutableArray.Create(
					new ExpectedFile(
						"Бронепоїзд (feat. Довгий Пес)",
						"Гоня & Довгий Пес",
						TimeSpan.Parse("00:02:06"))))
			{
				TestName = "Simple track",
			};
			const string secondAuthor = "Ницо Потворно";
			yield return new TestCaseData(
				"https://soundcloud.com/potvorno/sets/kyiv",
				ImmutableArray.Create(
					new ExpectedFile(
						"Київ",
						secondAuthor,
						TimeSpan.Parse("00:01:55")),
					new ExpectedFile(
						"Заспіваю ще",
						secondAuthor,
						TimeSpan.Parse("00:03:40"))))
			{
				TestName = "SoundCloud playlist",
			};
			const string thirdAuthor = "VISANCE";
			yield return new TestCaseData(
				"https://www.youtube.com/watch?v=ZtuXNrGeQ9s",
				ImmutableArray.Create(
					new ExpectedFile(
						"Drake - What's Next",
						thirdAuthor,
						TimeSpan.Parse("00:03:00")),
					new ExpectedFile(
						"Drake - Wants and Needs (ft. Lil Baby)",
						thirdAuthor,
						TimeSpan.Parse("00:03:13")),
					new ExpectedFile(
						"Drake - Lemon Pepper Freestyle (ft. Rick Ross)",
						thirdAuthor,
						TimeSpan.Parse("00:06:21"))))
			{
				TestName = "Track list in description",
			};
			yield return new TestCaseData(
				"https://youtu.be/1PYGkzyz_YM",
				ImmutableArray.Create(
					new ExpectedFile(
						"Глава 94 \"Gavno\"",
						"Stepan Glava",
						TimeSpan.Parse("00:03:30"))))
			{
				TestName = "Double quotes in file name",
			};
			yield return new TestCaseData(
				"https://youtu.be/kqrcUKehT_Y",
				ImmutableArray.Create(
					new ExpectedFile(
						"Зав'язав / Stage 13",
						"Глава 94",
						TimeSpan.Parse("00:03:40"))))
			{
				TestName = "Single quotes in file name",
			};
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_autoMock.Dispose();
			Directory.Delete(
				new DownloadOptions().CacheFilesFolderPath,
				recursive: true);
		}

		public record ExpectedFile(
			//string Name,
			string Title,
			string Author,
			TimeSpan Duration);
	}
}
