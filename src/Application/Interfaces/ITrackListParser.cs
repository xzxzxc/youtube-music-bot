using System.Collections.Generic;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Interfaces
{
	public interface ITrackListParser
	{
		IEnumerable<TrackModel> Parse(string text);
	}
}
