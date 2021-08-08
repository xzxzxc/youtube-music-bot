namespace YoutubeMusicBot.Console.Models
{
    public record MessageModel(
        int Id,
        ChatModel Chat,
        string Text,
        InlineButton? InlineButton);
}
