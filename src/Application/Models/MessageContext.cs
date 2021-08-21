namespace YoutubeMusicBot.Application.Models
{
    public record MessageContext(
        MessageModel UserMessage)
    {
        public MessageModel? MessageToUpdate { get; set; }

        public string? Title { get; set; }
    }
}
