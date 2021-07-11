namespace YoutubeMusicBot.Models
{
	public record CallbackQueryContext(
		ChatModel Chat,
		string? CallbackData);
}
