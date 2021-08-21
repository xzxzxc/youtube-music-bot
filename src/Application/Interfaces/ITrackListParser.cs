using System.Collections.Generic;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Interfaces
{
	public interface ITrackListParser
	{
		IEnumerable<TrackModel> Parse(string text);
	}
}
