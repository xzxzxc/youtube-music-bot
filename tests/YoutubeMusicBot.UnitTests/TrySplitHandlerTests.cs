using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.UnitTests
{
    public class TrySplitHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSplitTracksFromDescription(
            string description,
            IEnumerable<TrackModel> tracks,
            TrySplitHandler.Request request)
        {
            // TODO: add integration tests for real output
            var descriptionFile = Mock.Of<IFileInfo>(
                f => f.GetTextAsync(It.IsAny<CancellationToken>()).Result == description);
            var mp3SplitMock = new Mock<IMp3SplitWrapper>();
            var trackListParser = Mock.Of<ITrackListParser>(
                p => p.Parse(description) == tracks);
            var descriptionService = Mock.Of<IDescriptionService>(
                s => s.GetFileOrNull(request.File) == descriptionFile);

            var sut = new TrySplitHandler(mp3SplitMock.Object, trackListParser, descriptionService);

            var res = await sut.Handle(request);

            mp3SplitMock.Verify(
                s => s.SplitAsync(
                    request.File,
                    It.Is<IReadOnlyCollection<TrackModel>>(c => c.SequenceEqual(tracks)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            res.Should().BeTrue();
        }
    }
}
