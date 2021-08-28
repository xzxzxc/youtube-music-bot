using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Infrastructure.IntegrationTests.Helpers;
using MoreLinq.Extensions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;
using YoutubeMusicBot.Application.Models.YoutubeDownloader;
using YoutubeMusicBot.Infrastructure;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.Tests.Common;
using TagFile = TagLib.File;

namespace Infrastructure.IntegrationTests
{
    [Parallelizable]
    public class YoutubeDownloaderTests
    {
        private static readonly DirectoryInfo CacheFolder =
            new($"{nameof(YoutubeDownloaderTests)}_cache");

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        [Test]
        [Timeout(60_000)] // 60 seconds
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldReturnTracks(
            string url,
            IReadOnlyCollection<ExpectedTrack> expectedTracks)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                b =>
                {
                    b.RegisterModule(new CommonModule());
                });
            var sut = container.Create<YoutubeDownloader>();

            var res = await sut.DownloadAsync(CacheFolder.Name, url)
                .ToArrayAsync();

            var rawTitleChecks = expectedTracks
                .Where(t => t.RawTitle != null)
                .Select(
                    t => new Action<IDownloadResult>(
                        r => r.Should()
                            .BeOfType<RawTitleParsedResult>()
                            .Which.Value.Should()
                            .Be(t.RawTitle)));
            var fileChecks = expectedTracks
                .Select(
                    t => new Action<IDownloadResult>(
                        r =>
                        {
                            var file = r.Should()
                                .BeOfType<FileLoadedResult>()
                                .Which.Value;
                            var tagFile = TagFile.Create(file.FullName);
                            tagFile.Tag.Title.Should().Be(t.Title);
                            tagFile.Tag.FirstPerformer.Should().Be(t.Author);
                            tagFile.Properties.Duration.Should()
                                .BeCloseTo(t.Duration, precision: TimeSpan.FromSeconds(1));
                        }));
            res.Should().SatisfyRespectively(rawTitleChecks.Interleave(fileChecks));
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/wuROIJ0tRPU",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Гоня & Довгий Пес - Бронепоїзд",
                        "Бронепоїзд (feat. Довгий Пес)",
                        "Гоня & Довгий Пес",
                        TimeSpan.Parse("00:02:06")))) { TestName = "Simple track", };
            const string secondAuthor = "Ницо Потворно";
            yield return new TestCaseData(
                "https://soundcloud.com/potvorno/sets/kyiv",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Київ",
                        "Київ",
                        secondAuthor,
                        TimeSpan.Parse("00:01:55")),
                    new ExpectedTrack(
                        "Заспіваю ще",
                        "Заспіваю ще",
                        secondAuthor,
                        TimeSpan.Parse("00:03:40")))) { TestName = "SoundCloud playlist", };
            yield return new TestCaseData(
                "https://www.youtube.com/watch?v=rJ_rcbUB32Y&list=OLAK5uy_ksq4lX25NiCtiwvwPlG5cK1SvCfkp-Hrc",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Україна Кокаїна",
                        "Україна Кокаїна",
                        "Remafo",
                        TimeSpan.Parse("00:02:32")))) { TestName = "Youtube playlist", };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }

        public record ExpectedTrack(
            string? RawTitle,
            string Title,
            string Author,
            TimeSpan Duration);
    }
}
