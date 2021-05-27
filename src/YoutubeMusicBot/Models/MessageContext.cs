namespace YoutubeMusicBot.Models
{
    public record MessageContext(
        int Id,
        ChatContext Chat,
        string Text);
}
