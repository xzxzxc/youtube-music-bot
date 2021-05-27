namespace YoutubeMusicBot.Models
{
	public record CallbackQueryContext(
		ChatContext Chat,
		string? CallbackData);
}
