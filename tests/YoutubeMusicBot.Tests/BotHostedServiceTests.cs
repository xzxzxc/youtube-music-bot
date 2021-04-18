using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Sequences;
using MoreLinq;
using NUnit.Framework;
using Telegram.Bot.Types;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Tests.Stubs;
using YoutubeMusicBot.Wrappers;
using YoutubeMusicBot.Wrappers.Interfaces;
using TagFile = TagLib.File;

namespace YoutubeMusicBot.Tests
{
	[Parallelizable]
	public class BotHostedServiceTests
	{
		private Fixture _fixture = null!;
		private Mock<ITgClientWrapper> _clientWrapperMock = null!;
		private IContainer _container = null!;

		[SetUp]
		public void Setup()
		{
			Sequence.ContextMode = SequenceContextMode.Async;

			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_clientWrapperMock =
				new Mock<ITgClientWrapper>(MockBehavior.Strict);

			var builder = new ContainerBuilder();
			Program.ConfigureContainer(null, builder);
			// replaces real impl
			builder.RegisterGeneric(typeof(OptionsMonitorStub<>))
				.AsImplementedInterfaces();
			builder.RegisterGeneric(typeof(LoggerStub<>))
				.As(typeof(ILogger<>));
			builder.RegisterMock(_clientWrapperMock);

			_container = builder.Build();
		}

		[Test]
		[Timeout(60_000)] // 60 seconds
		[TestCaseSource(nameof(TestCases))]
		public async Task ShouldUploadAudioOnEcho(
			string url,
			IReadOnlyCollection<ExpectedFile> expectedFiles)
		{
			var message = _fixture
				.Build<Message>()
				.With(m => m.Text, url)
				.Create();
			var cacheFolder = _container
				.BeginMessageLifetimeScope(message.ToContext())
				.Resolve<ICacheFolder>()
				.Value;
			var resultTagFiles = new List<TagFile>();
			_clientWrapperMock
				.Setup(m => m.SendAudioAsync(It.IsAny<FileInfo>()))
				.Callback<FileInfo>(
					audio => resultTagFiles.Add(
						TagFile.Create(
							Path.Join(cacheFolder, audio.Name))))
				.ReturnsAsync(new Message());
			var messageHandler = _container.Resolve<MessageHandler>();

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
						//"NA-Гоня & Довгий Пес - Бронепоїзд.mp3",
						"Бронепоїзд (feat. Довгий Пес)",
						"Гоня & Довгий Пес",
						TimeSpan.Parse("00:02:06"))));
			const string secondAuthor = "Ницо Потворно";
			yield return new TestCaseData(
				"https://soundcloud.com/potvorno/sets/kyiv",
				ImmutableArray.Create(
					new ExpectedFile(
						//"1-Київ.mp3",
						"Київ",
						secondAuthor,
						TimeSpan.Parse("00:01:55")),
					new ExpectedFile(
						//"2-Заспіваю ще.mp3",
						"Заспіваю ще",
						secondAuthor,
						TimeSpan.Parse("00:03:40"))));
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
						TimeSpan.Parse("00:06:21"))));
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDown()
		{
			await _container.DisposeAsync();
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
