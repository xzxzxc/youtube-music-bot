namespace YoutubeMusicBot.Console.Interfaces
{
    public interface IDescriptionService
    {
        IFileInfo? GetFileOrNull(IFileInfo file);
    }
}
