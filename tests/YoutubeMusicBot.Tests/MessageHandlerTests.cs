﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;
using TagFile = TagLib.File;
using static FluentAssertions.FluentActions;

namespace YoutubeMusicBot.Tests
{
    public class MessageHandlerTests
    {
        private const string CacheFolderName = "cache";
        private const int TelegramMaxCallbackDataSize = 64;

        private readonly IFixture _fixture;
        private readonly IHost _host;
        private readonly Mock<ITgClientWrapper> _tgClientMock;
        private readonly IMediator _mediator;

        public MessageHandlerTests()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            _fixture = AutoFixtureFactory.Create();

            _tgClientMock = new Mock<ITgClientWrapper>
            {
                DefaultValueProvider = new AutoFixtureValueProvider(_fixture)
            };
            _host = Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, b) => { b.RegisterMock(_tgClientMock); })
                .UseSerilog(new LoggerConfiguration().WriteTo.InMemory().CreateLogger())
                .Build();

            var rootScope = _host.Services.GetRequiredService<ILifetimeScope>();
            _mediator = rootScope.Resolve<IMediator>();
        }

        [SetUp]
        public void SetUp()
        {
            var filesFromPrevRun = Directory.Exists(CacheFolderName)
                ? Directory.EnumerateFiles(
                    CacheFolderName,
                    "*",
                    SearchOption.AllDirectories)
                : Enumerable.Empty<string>();
            foreach (var filePath in filesFromPrevRun)
                File.Delete(filePath);

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

            await _mediator.Send(new MessageHandler.Request(message));

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);
            _tgClientMock.Verify(
                c => c.SendMessageAsync(
                    expectedMessage,
                    It.IsAny<InlineButton?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _tgClientMock.VerifyNoOtherCalls();
        }

        [Test]
        [TestCase(
            "https://youtu.be/wuROIJ0tRPU",
            "Loading started.",
            "Loading \"Гоня & Довгий Пес - Бронепоїзд\" started.")]
        public async Task ShouldSendLoadingStartedOnEcho(
            string url,
            params string[] expectedMessageTexts)
        {
            var message = _fixture
                .Build<MessageContext>()
                .With(m => m.Text, url)
                .Create();

            var replyMessage = _fixture.Create<MessageContext>();
            _tgClientMock.Setup(
                    c => c.SendMessageAsync(
                        expectedMessageTexts[0],
                        It.IsAny<InlineButton?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(replyMessage)
                .Verifiable();

            await _mediator.Send(new MessageHandler.Request(message));

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);

            _tgClientMock.VerifyAll();
            foreach (var expectedMessageText in expectedMessageTexts.Skip(1))
            {
                _tgClientMock.Verify(
                    c => c.UpdateMessageAsync(
                        replyMessage.Id,
                        expectedMessageText,
                        replyMessage.InlineButton,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        [Test]
        [TestCase("https://youtu.be/wuROIJ0tRPU")]
        public async Task ShouldDeleteMessageAfterFileSent(string url)
        {
            var message = _fixture
                .Build<MessageContext>()
                .With(m => m.Text, url)
                .Create();

            var replyMessage = _fixture.Create<MessageContext>();
            _tgClientMock.Setup(
                    m => m.SendMessageAsync(
                        It.Is<string>(s => s.StartsWith("Loading")),
                        It.IsAny<InlineButton?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(replyMessage);

            await _mediator.Send(new MessageHandler.Request(message));

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);

            _tgClientMock.Verify(
                c => c.DeleteMessageAsync(
                    replyMessage.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        [Timeout(10_000)] // 10 seconds
        [TestCase("https://youtu.be/wuROIJ0tRPU")]
        public async Task ShouldCancelLoadingUsingButton(string url)
        {
            var message = _fixture
                .Build<MessageContext>()
                .With(m => m.Text, url)
                .Create();
            var completionSource = new TaskCompletionSource<InlineButton?>();

            _tgClientMock.Setup(
                    m => m.SendMessageAsync(
                        It.Is<string>(s => s.StartsWith("Loading")),
                        It.Is<InlineButton?>(
                            ib => ib != null
                                && ib.Text == "Cancel"
                                && ib.CallbackData != null
                                && Encoding.Unicode.GetBytes(ib.CallbackData).Length
                                <= TelegramMaxCallbackDataSize),
                        It.IsAny<CancellationToken>()))
                .Callback<string, InlineButton?, CancellationToken>(
                    (_, ib, _) => completionSource.SetResult(ib))
                .ReturnsAsync(_fixture.Create<MessageContext>());

            var sendTask = _mediator.Send(new MessageHandler.Request(message));

            var inlineButton = await completionSource.Task;
            var callbackQuery = _fixture
                .Build<CallbackQueryContext>()
                .With(m => m.Chat, message.Chat)
                .With(m => m.CallbackData, inlineButton?.CallbackData)
                .Create();

            var sw = Stopwatch.StartNew();
            await _mediator.Send(new CallbackQueryHandler.Request(callbackQuery));
            await Awaiting(async () => await sendTask).Should().ThrowAsync<TaskCanceledException>();
            sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(0.1));

            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);
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
                .ReturnsAsync(_fixture.Create<MessageContext>());

            await _mediator.Send(new MessageHandler.Request(message));

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

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _host.Dispose();
        }

        public record ExpectedFile(string Title, string Author, TimeSpan Duration);
    }
}
