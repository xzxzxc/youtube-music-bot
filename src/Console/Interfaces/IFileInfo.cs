using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Console.Interfaces
{
    public interface IFileInfo
    {
        bool Exists { get; }

        string Name { get; }

        string DirectoryName { get; }

        long Length { get; }

        Task<string> GetTextAsync(
            CancellationToken cancellationToken = default);

        void Delete();

        Stream OpenRead();
    }
}
