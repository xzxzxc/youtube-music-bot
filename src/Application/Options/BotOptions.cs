namespace YoutubeMusicBot.Console.Options
{
	public class BotOptions
	{
		public string? Token { get; set; }

		public long MaxFileBytesCount { get; set; } = 50 * 1024 * 1024; // 50 mb
	}
}
