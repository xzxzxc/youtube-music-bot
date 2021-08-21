namespace YoutubeMusicBot.Application.Models
{
	public record CallbackQueryContext(
		ChatModel Chat,
		string? CallbackData);
}
