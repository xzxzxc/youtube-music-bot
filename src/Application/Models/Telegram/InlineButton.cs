namespace YoutubeMusicBot.Application.Models.Telegram
{
    public record InlineButton(string Text, string CallbackData)
    {
        public InlineButtonCollection ToCollection() =>
            new(this);
    }
}
