using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentAssertions;
using FluentAssertions.Extensions;
using MoreLinq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Models.Download;
using YoutubeMusicBot.Application.Models.Music;
using YoutubeMusicBot.Infrastructure.IntegrationTest.Helpers;
using YoutubeMusicBot.Infrastructure.Options;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;
using TagFile = TagLib.File;

namespace YoutubeMusicBot.Infrastructure.IntegrationTest
{
    public class MusicSplitterTests : BaseParallelizableWithTempFolderTest
    {
        [Test]
        [TestCaseSource(nameof(ShouldSplitByTrackListCases))]
        public async Task ShouldSplitByTrackList(string url, IReadOnlyList<Track> tracks)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var filePath = await DownloadFileAndGetPath(url, container);
            var sut = container.Create<MusicSplitter>();

            var trackPaths = await sut.SplitAsync(filePath, tracks).ToArrayAsync();

            trackPaths.Should().NotBeNull();
            var currentTrackStart = TimeSpan.Zero;
            var zip = tracks
                .ZipLongest(trackPaths, (expTrack, resPath) => (expTrack, resPath));
            foreach (var (expTrack, resPath) in zip)
            {
                expTrack.Should().NotBeNull();
                resPath.Should().NotBeNull();
                var tagFile = TagFile.Create(resPath);
                currentTrackStart.Should().BeCloseTo(expTrack.Start, 500.Milliseconds());
                tagFile.Tag.Title.Should().Be(expTrack.Title);

                currentTrackStart += tagFile.Properties.Duration;
            }
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/lfgWv3ypEIY", 13)]
        public async Task ShouldSplitBySilence(
            string url,
            int tracksCount)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var file = await DownloadFileAndGetPath(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitBySilenceAsync(file).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [TestCaseSource(nameof(ShouldNotTakeIntoAccountTooShortSilenceData))]
        public async Task ShouldNotTakeIntoAccountTooShortSilence(
            string url,
            int tracksCount,
            TimeSpan minSilenceLength)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                b => b.RegisterOptions(new SplitOptions { MinSilenceLength = minSilenceLength, }),
                initialize: true);
            var file = await DownloadFileAndGetPath(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitBySilenceAsync(file).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/wuROIJ0tRPU", 6)]
        public async Task ShouldSplitInEqualParts(
            string url,
            int tracksCount)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var file = await DownloadFileAndGetPath(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitInEqualPartsAsync(file, tracksCount).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [TestCaseSource(nameof(CornerCases))]
        public async Task ShouldWorkWithCornerCases(string url)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var file = await DownloadFileAndGetPath(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitInEqualPartsAsync(file, partsCount: 2).ToArrayAsync();

            tracks.Should().NotBeNull();
        }

        private async Task<string> DownloadFileAndGetPath(string url, AutoMock container)
        {
            var youtubeDlWrapper = container.Create<MusicDownloader>();
            return await youtubeDlWrapper.DownloadAsync(TempFolder.FullName, url)
                .OfType<FileLoadedResult>()
                .Select(r => r.MusicFilePath)
                .SingleAsync();
        }

        private static IEnumerable<TestCaseData> ShouldNotTakeIntoAccountTooShortSilenceData()
        {
            yield return new TestCaseData("https://youtu.be/lfgWv3ypEIY", 6, 1.Seconds());
        }

        private static IEnumerable<TestCaseData> ShouldSplitByTrackListCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/lfgWv3ypEIY",
                new[]
                {
                    new Track(TimeSpan.Zero, "first"), new Track(30.Seconds(), "second"),
                    new Track(1.Minutes() + 20.Seconds(), "third"),
                    new Track(1.Minutes() + 50.Seconds(), "fourth"),
                    new Track(2.Minutes() + 50.Seconds(), "fifth"),
                });
        }

        private static IEnumerable<TestCaseData> CornerCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/1PYGkzyz_YM") { TestName = "Double quotes in file name", };
            yield return new TestCaseData(
                "https://youtu.be/kqrcUKehT_Y") { TestName = "Single quotes in file name", };
        }
    }
}
