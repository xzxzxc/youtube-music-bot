using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Interfaces
{
	public interface ITrackFilesWatcher
	{
		string StartWatch(ChatContext chat);
	}
}
