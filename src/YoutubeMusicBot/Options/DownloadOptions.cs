using System;

namespace YoutubeMusicBot.Options
{
	internal class DownloadOptions
	{
		public string CacheFilesFolderPath { get; set; } = "cache";

		public TimeSpan TrackWatchDelay { get; set; } =
			TimeSpan.FromMilliseconds(150);
	}
}
