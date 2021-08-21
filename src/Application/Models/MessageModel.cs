namespace YoutubeMusicBot.Application.Models
{
    public record MessageModel(
        int Id,
        ChatModel Chat,
        string Text,
        InlineButton? InlineButton);
}
