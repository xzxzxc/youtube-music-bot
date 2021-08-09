using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Console.Handlers;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Options;
using YoutubeMusicBot.Console.Wrappers.Interfaces;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    [Parallelizable]
    public class TrySplitHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSplitTracksFromDescription(
            string description,
            TracksList tracks,
            IReadOnlyCollection<IFileInfo> files,
            TrySplitHandler.Request request)
        {
            // TODO: add integration tests for real output
            var descriptionFile = Mock.Of<IFileInfo>(
                f => f.GetTextAsync(It.IsAny<CancellationToken>()).Result == description);
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterMockOf<IDescriptionService>(
                        s => s.GetFileOrNull(request.File) == descriptionFile);
                    b.RegisterMockOf<ITrackListParser>(
                        p => p.Parse(description) == tracks);
                });
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitAsync(request.File, tracks, It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());

            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(Capture.In(newTrackRequests), It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
        }

        [Test]
        [CustomInlineAutoData]
        public async Task ShouldSplitFileBySilenceIfForced(IReadOnlyCollection<IFileInfo> files)
        {
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var fixture = AutoFixtureFactory.Create();
            var request = fixture.Build<TrySplitHandler.Request>()
                .With(r => r.ForceSplit, true)
                .Create();
            var container = AutoMockContainerFactory.Create();
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitBySilenceAsync(request.File, It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());
            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(Capture.In(newTrackRequests), It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
        }

        [Test]
        [CustomInlineAutoData(7, 3, 3)]
        public async Task ShouldSplitFileInEqualPartsIfForced(
            long fileLength,
            long maxFileSize,
            int fileParts,
            IReadOnlyCollection<IFileInfo> files)
        {
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var fixture = AutoFixtureFactory.Create();
            var file = Mock.Of<IFileInfo>(f => f.Length == fileLength);
            var request = fixture.Build<TrySplitHandler.Request>()
                .With(r => r.ForceSplit, true)
                .With(f => f.File, () => file)
                .Create();
            var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterOptions(new BotOptions { MaxFileBytesCount = maxFileSize, });
                });
            container.Mock<IMp3SplitWrapper>()
                .Setup(
                    w => w.SplitInEqualPartsAsync(
                        request.File,
                        fileParts,
                        It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());
            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(Capture.In(newTrackRequests), It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
        }
    }
}
