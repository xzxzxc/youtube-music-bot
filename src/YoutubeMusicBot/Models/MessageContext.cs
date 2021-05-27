namespace YoutubeMusicBot.Models
{
    public record MessageContext(
        int Id,
        ChatContext Chat,
        string Text)
    {
        public int? MessageToUpdateId { get; set; }
    }
}
