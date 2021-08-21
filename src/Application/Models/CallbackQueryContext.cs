namespace YoutubeMusicBot.Console.Models
{
	public record CallbackQueryContext(
		ChatModel Chat,
		string? CallbackData);
}
