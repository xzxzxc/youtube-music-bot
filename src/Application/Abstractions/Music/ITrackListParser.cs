using System.Collections.Generic;
using YoutubeMusicBot.Application.Models.Music;

namespace YoutubeMusicBot.Application.Abstractions.Music
{
	public interface ITrackListParser
	{
		IEnumerable<Track> Parse(string text);
	}
}
