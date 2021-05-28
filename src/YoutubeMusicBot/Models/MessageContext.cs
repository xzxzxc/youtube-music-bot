namespace YoutubeMusicBot.Models
{
    public record MessageContext(
        int Id,
        ChatContext Chat,
        string Text,
        InlineButton? InlineButton)
    {
        public MessageContext? MessageToUpdate { get; set; }
    }
}
