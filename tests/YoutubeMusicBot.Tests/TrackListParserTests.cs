using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Tests
{
	public class TrackListParserTests
	{
		[TestCaseSource(nameof(ParseData))]
		public void ShouldParseDescription(
			string description,
			IEnumerable<TrackModel> expectedModels)
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
					new TrackModel(TimeSpan.Zero, "test 1"),
					new TrackModel(
						TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(2),
						"test 2"),
					new TrackModel(
						TimeSpan.FromHours(1)
						+ TimeSpan.FromMinutes(7)
						+ TimeSpan.FromSeconds(5),
						"test 3"),
				});
		}
	}
}
