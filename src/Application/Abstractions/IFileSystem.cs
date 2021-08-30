using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Abstractions
{
    public interface IFileSystem
    {
        string GetOrCreateTempFolder<T>(T folderId);

        ValueTask RemoveTempFolderAndContent<T>(T folderId);

        Task<string> GetFileTextAsync(string filePath, CancellationToken cancellationToken);

        long GetFileBytesCount(string filePath);

        Stream OpenReadStream(string filePath);

        string GetFileName(string filePath);

        string GetFileDirectoryPath(string filePath);

        string JoinPath(params string[] pathParts);

        string ChangeExtension(string fileName, string extension);

        bool IsFileExists(string filePath);
    }
}
