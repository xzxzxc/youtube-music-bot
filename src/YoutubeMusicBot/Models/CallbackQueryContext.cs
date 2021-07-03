namespace YoutubeMusicBot.Models
{
	public record CallbackQueryContext(
		ChatContext Chat,
		byte[] CallbackData);
}
