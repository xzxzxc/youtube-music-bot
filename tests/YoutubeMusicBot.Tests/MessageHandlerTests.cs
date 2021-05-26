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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;
using TagFile = TagLib.File;

namespace YoutubeMusicBot.Tests
{
	public class MessageHandlerTests
	{
		private const string CacheFolderName = "cache";

		private readonly Fixture _fixture;
		private readonly IHost _host;
		private readonly ILifetimeScope _rootScope;
		private readonly Mock<ITgClientWrapper> _tgClientMock;

		public MessageHandlerTests()
		{
			Sequence.ContextMode = SequenceContextMode.Async;

			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_tgClientMock = new Mock<ITgClientWrapper>();
			_host = Program.CreateHostBuilder()
				.ConfigureContainer<ContainerBuilder>(
					(_, b) => { b.RegisterMock(_tgClientMock); })
				.UseSerilog(new LoggerConfiguration().WriteTo.InMemory().CreateLogger())
				.Build();

			_rootScope = _host.Services.GetRequiredService<ILifetimeScope>();
		}

		[SetUp]
		public async Task SetUp()
		{
			if (Directory.Exists(CacheFolderName))
			{
				// idk why, but folder could be locked, so wait a little bit
				await Task.Delay(TimeSpan.FromMilliseconds(30));
				Directory.Delete(CacheFolderName, recursive: true);
			}

			_tgClientMock.Reset();
			InMemorySink.Instance.Dispose(); // this would clear all events
		}

		[Test]
		[TestCase("", "Message must be not empty.")]
		[TestCase("kljjk", "Message must be valid URL.")]
		[TestCase("htt://test.com", "Message must be valid URL.")]
		[TestCase("http:/test.com", "Message must be valid URL.")]
		[TestCase("test.com", "Message must be valid URL.")]
		public async Task ShouldSendErrorMessageOnInvalidUrl(
			string url,
			string expectedMessage)
		{
			var message = _fixture
				.Build<MessageContext>()
				.With(m => m.Text, url)
				.Create();
			var mediator = _rootScope.Resolve<IMediator>();

			await mediator.Send(new MessageHandler.Request(message));

			InMemorySink.Instance.LogEvents.Should()
				.NotContain(e => e.Level >= LogEventLevel.Error);
			_tgClientMock.Verify(
				c => c.SendMessageAsync(expectedMessage, It.IsAny<CancellationToken>()),
				Times.Once);
			_tgClientMock.VerifyNoOtherCalls();
		}

		[Test]
		[TestCase(
			"https://youtu.be/wuROIJ0tRPU",
			"Loading \"Гоня & Довгий Пес - Бронепоїзд\" started.")]
		public async Task ShouldSendLoadingStartedOnEcho(
			string url,
			string expectedMessage)
		{
			var message = _fixture
				.Build<MessageContext>()
				.With(m => m.Text, url)
				.Create();
			var mediator = _rootScope.Resolve<IMediator>();

			await mediator.Send(new MessageHandler.Request(message));

			InMemorySink.Instance.LogEvents.Should()
				.NotContain(e => e.Level >= LogEventLevel.Error);
			_tgClientMock.Verify(
				c => c.SendMessageAsync(expectedMessage, It.IsAny<CancellationToken>()),
				Times.Once);
		}

		[Test]
		[Timeout(120_000)] // 2 minutes
		[TestCaseSource(nameof(TestCases))]
		public async Task ShouldUploadAudioOnEcho(
			string url,
			IReadOnlyCollection<ExpectedFile> expectedFiles)
		{
			var message = _fixture
				.Build<MessageContext>()
				.With(m => m.Text, url)
				.Create();
			var resultTagFiles = new List<TagFile>();
			_tgClientMock
				.Setup(m => m.SendAudioAsync(It.IsAny<FileInfo>(), It.IsAny<CancellationToken>()))
				.Callback<FileInfo, CancellationToken>(
					(audio, _) =>
						resultTagFiles.Add(
							TagFile.Create(
								Path.Join(CacheFolderName, $"{message.Chat.Id}", audio.Name))))
				.ReturnsAsync(new Message());
			var mediator = _rootScope.Resolve<IMediator>();

			await mediator.Send(new MessageHandler.Request(message));

			InMemorySink.Instance.LogEvents.Should()
				.NotContain(e => e.Level >= LogEventLevel.Error);
			resultTagFiles.Should().HaveSameCount(expectedFiles);
			foreach (var (expected, result) in expectedFiles
				.Zip(resultTagFiles))
			{
				result.Tag.Title.Should().Be(expected.Title);
				result.Tag.FirstPerformer.Should().Be(expected.Author);
				result.Properties.Duration.Should()
					.BeCloseTo(expected.Duration, precision: TimeSpan.FromSeconds(1));
			}

			Directory.EnumerateFileSystemEntries(Path.Join(CacheFolderName, $"{message.Chat.Id}"))
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
			_host.Dispose();
		}

		public record ExpectedFile(string Title, string Author, TimeSpan Duration);
	}
}
