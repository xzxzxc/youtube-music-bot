using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Models.Music;
using YoutubeMusicBot.Application.Services;

namespace YoutubeMusicBot.Application.UnitTests.Services
{
    [Parallelizable]
    public class TrackListParserTests
    {
        [Parallelizable]
        [TestCaseSource(nameof(ParseData))]
        public void ShouldParseDescription(
            string description,
            IEnumerable<Track> expectedModels)
        {
            var parser = new TrackListParser();

            var res = parser.Parse(description);

            res.Should().Equal(expectedModels);
        }

        public static IEnumerable<TestCaseData> ParseData()
        {
            yield return new TestCaseData(
                @"
0:00 test 1
05:02 test 2
01:07:05 test 3",
                new[]
                {
                    new Track(TimeSpan.Zero, "test 1"),
                    new Track(5.Minutes() + 2.Seconds(), "test 2"),
                    new Track(1.Hours() + 7.Minutes() + 5.Seconds(), "test 3"),
                });
            yield return new TestCaseData(
                @"
01 Intro 00:00:00
02 Двоє 00:03:37
03 2 01:04:55
",
                new[]
                {
                    new Track(TimeSpan.Zero, "01 Intro"),
                    new Track(3.Minutes() + 37.Seconds(), "02 Двоє"),
                    new Track(1.Hours() + 4.Minutes() + 55.Seconds(), "03 2"),
                });
            yield return new TestCaseData(
                @"
Двоє чи троє 00:03:37",
                new[] { new Track(3.Minutes() + 37.Seconds(), "Двоє чи троє"), });
        }
    }
}
