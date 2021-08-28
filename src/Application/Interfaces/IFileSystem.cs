namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IFileSystem
    {
        string CreateTempFolder<T>(T folderId);
    }
}
