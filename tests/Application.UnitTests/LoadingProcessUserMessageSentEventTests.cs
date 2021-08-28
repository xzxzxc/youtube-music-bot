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
using YoutubeMusicBot.UnitTests.Helpers;

namespace YoutubeMusicBot.UnitTests
{
    public class LoadingProcessUserMessageSentEventTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldUpdateUserMessageOnTrackRawTitleParsed(
            LoadingProcessMessageSentEvent @event,
            IReadOnlyList<RawTitleParsedResult> titles)
        {
            using var container = CreateContainerFor(@event, titles);
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
                lazyCapture.Value.VerifyCancelButton(@event);
            }
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseNewTrackOnEachNewTrackLoaded(
            LoadingProcessMessageSentEvent @event,
            string cacheFolder,
            IReadOnlyList<FileLoadedResult> tracks)
        {
            using var container = CreateContainerFor(@event, tracks);
            var sut = container.Create<LoadingProcessUserMessageSentHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            foreach (var (uncommittedEvent, file) in uncommittedEvents.Zip(
                tracks.Select(t => t.Value)))
            {
                uncommittedEvent.Should()
                    .BeOfType<NewMusicFileEvent>()
                    .Which.FullPath.Should()
                    .Be(file.FullName);
            }
        }

        private static AutoMock CreateContainerFor(
            LoadingProcessMessageSentEvent messageValidEvent,
            IReadOnlyList<IDownloadResult> tracks)
        {
            var cacheFolder = AutoFixtureFactory.Create().Create<string>();
            var res = AutoMockContainerFactory.Create();
            res.Mock<IFileSystem>()
                .Setup(s => s.CreateTempFolder($"{messageValidEvent.Id}"))
                .Returns(cacheFolder);
            res.Mock<IYoutubeDownloader>()
                .Setup(
                    y => y.DownloadAsync(
                        cacheFolder,
                        messageValidEvent.Aggregate.Text,
                        It.IsAny<CancellationToken>()))
                .Returns(tracks.ToAsyncEnumerable());
            return res;
        }
    }
}
