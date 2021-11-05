using System;
using System.Collections.Generic;
using FluentAssertions;
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
					new Track(
						TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(2),
						"test 2"),
					new Track(
						TimeSpan.FromHours(1)
						+ TimeSpan.FromMinutes(7)
						+ TimeSpan.FromSeconds(5),
						"test 3"),
				});
            yield return new TestCaseData(
            @"
01 Intro 00:00:00
02 Двоє 00:03:37",
            new[]
            {
                new Track(TimeSpan.Zero, "01 Intro"),
                new Track(
                    TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(37),
                    "02 Двоє"),
            });
            yield return new TestCaseData(
                @"
Двоє чи троє 00:03:37",
                new[]
                {
                    new Track(
                        TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(37),
                        "Двоє чи троє"),
                });
		}
	}
}
