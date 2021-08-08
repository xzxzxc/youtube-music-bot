using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Services
{
	public class TrackListParser : ITrackListParser
	{
		private static readonly Regex Regex = new(
			@"^
# first option
	# special chars
	(?:.*?[-\| \\_\.\[\]\(\)\{\}\""]+)?
	# start time
	(?:(?<time1>(?:\d{1,2}\:)?\d{1,2}\:\d\d)
	# special chars
	[-\|\ \\_\.\[\]\(\)\{\}\""]+
	# name
	(?<name1>.+)

#second option
	# name
	|(?<name2>.+)
	# special chars
	[-\|\\_\.\[\]\(\)\{\}\""]+
	# start time
	(?<time2>(?:\d{1,2}\:)?\d{1,2}\:\d\d))
	# other
	.*
$",
			RegexOptions.Compiled
			| RegexOptions.IgnorePatternWhitespace
			| RegexOptions.Multiline);

		public IEnumerable<TrackModel> Parse(string text)
		{
			foreach (Match match in Regex.Matches(text))
			{
				if (!match.Success)
					continue;

				var name = (match.Groups.GetValueOrDefault("name1")
					?? match.Groups.GetValueOrDefault("name2"))?.Value;
				var time = (match.Groups.GetValueOrDefault("time1")
					?? match.Groups.GetValueOrDefault("time2"))?.Value;

				if (string.IsNullOrEmpty(name)
					|| string.IsNullOrEmpty(time))
				{
					continue;
				}

				yield return new TrackModel(
					ParseTime(time),
					name);
			}
		}

		private static TimeSpan ParseTime(string time)
		{
			// transform 'm:s' to 'h:m:s'
			if (time.Count(c => c == ':') < 2)
				time = "0:" + time;

			return TimeSpan.Parse(time);
		}
	}
}
