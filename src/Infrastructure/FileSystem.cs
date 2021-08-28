using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Infrastructure
{
    public class FileSystem : IFileSystem
    {
        private readonly IOptionsMonitor<FileSystemOptions> _options;

        public FileSystem(IOptionsMonitor<FileSystemOptions> options)
        {
            _options = options;
        }

        public string GetOrCreateTempFolder<T>(T folderId)
        {
            var folderPath = GetTempFolderPath(folderId);
            var folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                folder.Create();

            return folder.FullName;
        }

        public void RemoveTempFolderAndContent<T>(T folderId) =>
            Directory.Delete(GetTempFolderPath(folderId), recursive: true);

        public Task<string> GetFileTextAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            File.ReadAllTextAsync(filePath, cancellationToken);

        public long GetFileBytesCount(string filePath) =>
            new FileInfo(filePath).Length;

        public Stream OpenReadStream(string filePath) =>
            File.OpenRead(filePath);

        public string GetFileName(string filePath) =>
            new FileInfo(filePath).Name;

        public string GetFileDirectoryPath(string filePath) =>
            new FileInfo(filePath).DirectoryName
            ?? throw new InvalidOperationException("Unable to get directory");

        public string JoinPath(params string[] pathParts) =>
            Path.Join(pathParts);

        public string ChangeExtension(string fileName, string extension) =>
            Path.ChangeExtension(fileName, extension);

        public bool IsFileExists(string filePath) =>
            File.Exists(filePath);

        private string GetTempFolderPath<T>(T folderId) =>
            Path.Join(_options.CurrentValue.TempFolderPath, $"{folderId}");
    }
}
