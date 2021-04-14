using System.Collections.Generic;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
	public interface ITrackListParser
	{
		IEnumerable<TrackModel> Parse(string text);
	}
}
