using System.Collections.Generic;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Interfaces
{
	public interface ITrackListParser
	{
		IEnumerable<TrackModel> Parse(string text);
	}
}
