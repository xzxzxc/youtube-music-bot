using System.ComponentModel.DataAnnotations;

namespace YoutubeMusicBot.Application.Options
{
	public class BotOptions
    {
        [Required]
        public string Token { get; set; } = string.Empty;

		public long FileBytesLimit { get; set; } = 50 * 1024 * 1024; // 50 mb
	}
}
