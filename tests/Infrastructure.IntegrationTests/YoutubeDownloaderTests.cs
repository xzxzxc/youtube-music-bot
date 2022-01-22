using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MoreLinq.Extensions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Download;
using YoutubeMusicBot.Application.Models.Download;
using YoutubeMusicBot.Infrastructure.IntegrationTest.Helpers;
using YoutubeMusicBot.IntegrationTests.Common;
using TagFile = TagLib.File;

namespace YoutubeMusicBot.Infrastructure.IntegrationTest
{
    public class YoutubeDownloaderTests : BaseParallelizableWithTempFolderTest
    {
        [Test]
        [Timeout(60_000)] // 60 seconds
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldReturnTracks(
            string url,
            IReadOnlyCollection<ExpectedTrack> expectedTracks)
        {
            using var container = await AutoMockInfrastructureContainerFactory.Create(
                initialize: true);
            var sut = container.Create<MusicDownloader>();

            var res = await sut.DownloadAsync(TempFolder.FullName, url)
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
                            var result = r.Should()
                                .BeOfType<FileLoadedResult>()
                                .Which;
                            var tagFile = TagFile.Create(result.MusicFilePath);
                            tagFile.Tag.Title.Should().Be(t.Title);
                            tagFile.Tag.FirstPerformer.Should().Be(t.Author);
                            tagFile.Properties.Duration.Should()
                                .BeCloseTo(t.Duration, precision: TimeSpan.FromSeconds(1));
                            result.DescriptionFilePath.Should().NotBeNull();
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
                        TimeSpan.Parse("00:02:06"),
                        DescriptionExists: true))) { TestName = "Simple track", };
            const string secondAuthor = "Ницо Потворно";
            yield return new TestCaseData(
                "https://soundcloud.com/potvorno/sets/kyiv",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Київ",
                        "Київ",
                        secondAuthor,
                        TimeSpan.Parse("00:01:55"),
                        DescriptionExists: true),
                    new ExpectedTrack(
                        "Заспіваю ще",
                        "Заспіваю ще",
                        secondAuthor,
                        TimeSpan.Parse("00:03:40"),
                        DescriptionExists: true))) { TestName = "SoundCloud playlist", };
            yield return new TestCaseData(
                "https://www.youtube.com/watch?v=rJ_rcbUB32Y&list=OLAK5uy_ksq4lX25NiCtiwvwPlG5cK1SvCfkp-Hrc",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Україна Кокаїна",
                        "Україна Кокаїна",
                        "Remafo",
                        TimeSpan.Parse("00:02:32"),
                        DescriptionExists: true))) { TestName = "Youtube playlist", };
        }

        public record ExpectedTrack(
            string? RawTitle,
            string Title,
            string Author,
            TimeSpan Duration,
            bool DescriptionExists);
    }
}
