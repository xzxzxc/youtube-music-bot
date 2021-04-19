namespace YoutubeMusicBot.Models
{
	public record MessageContext(
		ChatContext Chat,
		string Text)
	{
	}
}
