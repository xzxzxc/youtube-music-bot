using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using AutoFixture;
using System.Threading.Tasks;
using Autofac;
using AutoFixture.AutoMoq;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Infrastructure.Database;
using TagFile = TagLib.File;
using static Application.IntegrationTests.CommonFixture;

namespace Application.IntegrationTests
{
    [Parallelizable]
    public class MessageHandlerTests
    {
        private readonly IMediator _mediator;
        private readonly MessageModel _validMessage;

        public MessageHandlerTests()
        {
            _validMessage = FixtureInstance
                .Build<MessageModel>()
                .With(m => m.Text, "https://youtu.be/wuROIJ0tRPU")
                .Create();

            _mediator = RootScope.Resolve<IMediator>();
        }

        [SetUp]
        public void SetUp()
        {
            var filesFromPrevRun = CacheFolder.Exists
                ? CacheFolder.EnumerateFiles(
                    "*",
                    SearchOption.AllDirectories)
                : Enumerable.Empty<FileInfo>();
            foreach (var fileInfo in filesFromPrevRun)
                fileInfo.Delete();

            TgClientMock.Reset();
            InMemorySink.Instance.Dispose(); // this would clear all events
        }

        [Test]
        public async Task ShouldCreateMessageAggregate()
        {
            var dbContext = RootScope.Resolve<ApplicationDbContext>();
            var sut = RootScope.Resolve<MessageHandler>();

            await sut.Handle(new MessageHandler.Request(_validMessage));

            var message = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Messages,
                m => m.ExternalId == _validMessage.Id);
            message.Should().NotBeNull();
        }

        [Test]
        [Timeout(120_000)] // 2 minutes
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldUploadAudioOnEcho(
            string url,
            IReadOnlyCollection<ExpectedFile> expectedFiles)
        {
            var message = FixtureInstance
                .Build<MessageModel>()
                .With(m => m.Text, url)
                .Create();
            var resultTagFiles = new List<TagFile>();

            TgClientMock.Setup(m => m.SendAudioAsync(It.IsAny<IFileInfo>(), It.IsAny<CancellationToken>()))
                .Callback<IFileInfo, CancellationToken>(
                    (audio, _) =>
                        resultTagFiles.Add(TagFile.Create(audio.FullName)))
                .ReturnsUsingFixture(FixtureInstance);

            await _mediator.Send(new MessageHandler.Request(message));

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);

            resultTagFiles.Should().HaveSameCount(expectedFiles);
            foreach (var (expected, tagFile) in expectedFiles
                .Zip(resultTagFiles))
            {
                tagFile.Tag.Title.Should().Be(expected.Title);
                tagFile.Tag.FirstPerformer.Should().Be(expected.Author);
                tagFile.Properties.Duration.Should()
                    .BeCloseTo(expected.Duration, precision: TimeSpan.FromSeconds(1));
            }

            CheckDirectoryIsEmpty(message.Chat.Id);
        }


        [Test]
        [Timeout(30_000)] // 30 seconds
        [TestCase("https://youtu.be/wuROIJ0tRPU")]
        public async Task ShouldCancelLoadingUsingButton(string url)
        {
            var message = FixtureInstance
                .Build<MessageModel>()
                .With(m => m.Text, url)
                .Create();
            var completionSource = new TaskCompletionSource<InlineButton?>();

            TgClientMock.Setup(
                    m => m.SendMessageAsync(
                        It.Is<string>(s => s.StartsWith("Loading")),
                        It.IsAny<InlineButtonCollection>(),
                        It.IsAny<CancellationToken>()))
                .Callback<string, InlineButtonCollection, CancellationToken>(
                    (_, ib, _) => completionSource.SetResult(ib.First()))
                .ReturnsAsync(FixtureInstance.Create<MessageModel>());

            var sendTask = _mediator.Send(new MessageHandler.Request(message));

            var inlineButton = await completionSource.Task;

            var callbackQuery = FixtureInstance
                .Build<CallbackQueryContext>()
                .With(m => m.Chat, message.Chat)
                .With(m => m.CallbackData, inlineButton?.CallbackData)
                .Create();

            await _mediator.Send(new CallbackQueryHandler.Request(callbackQuery));
            await FluentActions.Awaiting(() => sendTask)
                .Should()
                .ThrowAsync<OperationCanceledException>();

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);
            // CheckDirectoryIsEmpty(message.Chat.Id); // TODO: fix this
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/wuROIJ0tRPU",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Бронепоїзд (feat. Довгий Пес)",
                        "Гоня & Довгий Пес",
                        TimeSpan.Parse("00:02:06")))) { TestName = "Simple track", };
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
                        TimeSpan.Parse("00:03:40")))) { TestName = "SoundCloud playlist", };
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
                        TimeSpan.Parse("00:06:21")))) { TestName = "Track list in description", };
            yield return new TestCaseData(
                "https://youtu.be/1PYGkzyz_YM",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Глава 94 \"Gavno\"",
                        "Stepan Glava",
                        TimeSpan.Parse("00:03:30")))) { TestName = "Double quotes in file name", };
            yield return new TestCaseData(
                "https://youtu.be/kqrcUKehT_Y",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Зав'язав / Stage 13",
                        "Глава 94",
                        TimeSpan.Parse("00:03:40")))) { TestName = "Single quotes in file name", };
        }

        private static void CheckDirectoryIsEmpty(long chatId)
        {
            Directory.EnumerateFileSystemEntries(Path.Join(CacheFolder.FullName, $"{chatId}"))
                .Should()
                .BeEmpty();
        }

        public record ExpectedFile(string Title, string Author, TimeSpan Duration);
    }
}
