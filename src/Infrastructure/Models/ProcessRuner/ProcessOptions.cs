namespace YoutubeMusicBot.Infrastructure.Models.ProcessRuner
{
    public record ProcessOptions(
        string ProcessName,
        string WorkingDirectory,
        params string[] Arguments);
}
