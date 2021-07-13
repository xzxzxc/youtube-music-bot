using System.IO;

namespace YoutubeMusicBot.Interfaces
{
    public interface IFileInfo
    {
        bool Exists { get; }

        string Name { get; }

        string DirectoryName { get; }

        long Length { get; }

        void Delete();

        Stream OpenRead();
    }
}
