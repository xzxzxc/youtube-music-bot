namespace YoutubeMusicBot.Options
{
	internal class BotOptions
	{
		public string Token { get; set; }

		public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50 mb
	}
}
