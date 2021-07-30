namespace YoutubeMusicBot.Interfaces
{
    public interface IDescriptionService
    {
        IFileInfo? GetFileOrNull(IFileInfo file);
    }
}
