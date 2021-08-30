using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Models.Download;
using YoutubeMusicBot.Infrastructure.IntegrationTest.Helpers;
using YoutubeMusicBot.Infrastructure.Options;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;

namespace YoutubeMusicBot.Infrastructure.IntegrationTest
{
    [Parallelizable]
    public class MusicSplitterTests
    {
        public static DirectoryInfo CacheFolder = new($"{nameof(MusicSplitterTests)}_tests_cache");

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/lfgWv3ypEIY", 13)]
        public async Task ShouldSplitBySilence(
            string url,
            int tracksCount)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var file = await DownloadFile(url, container);
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
            var file = await DownloadFile(url, container);
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
            var file = await DownloadFile(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitInEqualPartsAsync(file, tracksCount).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        private static async Task<string> DownloadFile(string url, AutoMock container)
        {
            var youtubeDlWrapper = container.Create<MusicDownloader>();
            return await youtubeDlWrapper.DownloadAsync(CacheFolder.FullName, url)
                .OfType<FileLoadedResult>()
                .Select(r => r.MusicFilePath)
                .SingleAsync();
        }

        private static IEnumerable<TestCaseData> ShouldNotTakeIntoAccountTooShortSilenceData()
        {
            yield return new TestCaseData("https://youtu.be/lfgWv3ypEIY", 6, 1.Seconds());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }
    }
}
