namespace YoutubeMusicBot.Application.Options
{
	public class BotOptions
	{
		public string? Token { get; set; }

		public long FileBytesLimit { get; set; } = 50 * 1024 * 1024; // 50 mb
	}
}
