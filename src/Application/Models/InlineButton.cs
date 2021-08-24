namespace YoutubeMusicBot.Application.Models
{
    public record InlineButton(string Text, string? CallbackData)
    {
        public InlineButtonCollection ToCollection() =>
            new(this);
    }
}
