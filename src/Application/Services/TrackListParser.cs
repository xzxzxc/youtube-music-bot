using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeMusicBot.Application.Abstractions.Music;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Models.Music;

namespace YoutubeMusicBot.Application.Services
{
    public class TrackListParser : ITrackListParser
    {
        private static readonly Regex Regex = new(
            @"^
# first option
	# special chars
	(?:.*?[-\| \\_\.\[\]\(\)\{\}\""]+)?
	# start time
	(?<time1>(?:\d{1,2}\:)?\d{1,2}\:\d\d)
	# delimiters
	[-\|\ \\_\.\[\]\(\)\{\}\""]+
	# name
	(?<name1>.+)

#second option
	# name
	|(?:(?<name2>.+)
	# delimiters
	[-\|\ \\_\.\[\]\(\)\{\}\""]+
	# start time
	(?<time2>(?:\d{1,2}\:)?\d{1,2}\:\d\d))
	# other
	.*
$",
            RegexOptions.Compiled
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Multiline);

        public IEnumerable<Track> Parse(string text)
        {
            foreach (Match match in Regex.Matches(text))
            {
                if (!match.Success)
                    continue;

                string name, time;
                if (match.Groups.TryGetValue("name1", out var nameGroup)
                    && nameGroup.Success
                    && match.Groups.TryGetValue("time1", out var timeGroup)
                    && timeGroup.Success)
                {
                    name = nameGroup.Value;
                    time = timeGroup.Value;
                }
                else if (match.Groups.TryGetValue("name2", out nameGroup)
                    && nameGroup.Success
                    && match.Groups.TryGetValue("time2", out timeGroup)
                    && timeGroup.Success)
                {
                    name = nameGroup.Value;
                    time = timeGroup.Value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Match was successful but name wasn't parsed");
                }

                if (string.IsNullOrEmpty(name))
                    throw new InvalidOperationException("Parsed empty name");

                if (string.IsNullOrEmpty(time))
                    throw new InvalidOperationException("Parsed empty time");

                yield return new Track(ParseTime(time), name);
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
