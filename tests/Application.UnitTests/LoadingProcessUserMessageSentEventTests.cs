using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Models.YoutubeDownloader;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.UnitTests.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    public class LoadingProcessUserMessageSentEventTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCreateTempFolder(
            LoadingProcessMessageSentEvent @event,
            string cacheFolder)
        {
            using var container = CreateContainerFor(@event, cacheFolder: cacheFolder);
            var sut = container.Create<LoadingProcessUserMessageSentHandler>();

            await sut.Handle(@event);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldUpdateUserMessageOnTrackRawTitleParsed(
            LoadingProcessMessageSentEvent @event,
            string callbackData,
            IReadOnlyList<RawTitleParsedResult> titles)
        {
            using var container = CreateContainerFor(@event, titles);
            container.Mock<ICallbackDataFactory>()
                .Setup(c => c.CreateForCancel(@event))
                .Returns(callbackData);
            var sut = container.Create<LoadingProcessUserMessageSentHandler>();

            await sut.Handle(@event);

            var lazyCapture = new LazyCapture<InlineButtonCollection>();
            foreach (var title in titles.Select(t => t.Value))
            {
                container.Mock<ITgClient>()
                    .Verify(
                        c => c.UpdateMessageAsync(
                            @event.Aggregate.ChatId,
                            @event.MessageId,
                            $"Loading \"{title}\" started.",
                            Capture.With(lazyCapture.Match),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
                lazyCapture.Value.VerifyCancelButton(callbackData);
            }
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMusicFileCreatedOnEachTrackLoaded(
            LoadingProcessMessageSentEvent @event,
            string cacheFolder,
            IReadOnlyList<FileLoadedResult> tracks)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = CreateContainerFor(@event, tracks);
            var sut = container.Create<LoadingProcessUserMessageSentHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            foreach (var (resEvent, (musicFile, descrFile)) in uncommittedEvents.Zip(tracks))
            {
                var musicFileCreatedEvent = resEvent.Should()
                    .BeOfType<MusicFileCreatedEvent>()
                    .Which;
                musicFileCreatedEvent.MusicFilePath.Should().Be(musicFile);
                musicFileCreatedEvent.DescriptionFilePath.Should().Be(descrFile);
            }

            container.VerifyMessageSaved(@event.Aggregate, Times.AtLeast(tracks.Count));
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMessageFinishedEvent(
            LoadingProcessMessageSentEvent @event)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = CreateContainerFor(@event);
            var sut = container.Create<LoadingProcessUserMessageSentHandler>();

            await sut.Handle(@event);

            var lastEvent = @event.Aggregate.GetUncommittedEvents().LastOrDefault();
            lastEvent.Should().NotBeNull();
            lastEvent.Should().BeOfType<MessageFinishedEvent>();
            container.VerifyMessageSaved(@event.Aggregate);
        }

        private static AutoMock CreateContainerFor(
            LoadingProcessMessageSentEvent messageValidEvent,
            IReadOnlyList<IDownloadResult>? tracks = null,
            string? cacheFolder = null)
        {
            cacheFolder ??= AutoFixtureFactory.Create().Create<string>();
            var res = AutoMockContainerFactory.Create();

            res.Mock<IFileSystem>()
                .Setup(s => s.GetOrCreateTempFolder(messageValidEvent.AggregateId))
                .Returns(cacheFolder);

            if (tracks != null)
            {
                res.Mock<IYoutubeDownloader>()
                    .Setup(
                        y => y.DownloadAsync(
                            cacheFolder,
                            messageValidEvent.Aggregate.Text,
                            It.IsAny<CancellationToken>()))
                    .Returns(tracks.ToAsyncEnumerable());
            }

            return res;
        }
    }
}
