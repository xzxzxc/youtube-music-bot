namespace YoutubeMusicBot.Infrastructure.Models.ProcessRuner
{
    public record ProcessResultLine(string Value, bool IsError = false)
    {
        public static implicit operator string(ProcessResultLine line) =>
            line.Value;
    }
}
