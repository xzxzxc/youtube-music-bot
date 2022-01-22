using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Infrastructure.Options;

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

        public async ValueTask RemoveTempFolderAndContent<T>(T folderId)
        {
            Stopwatch? stopwatch = null;
            var directoryInfo = new DirectoryInfo(GetTempFolderPath(folderId));
            while (!TryDeleteDirectory(directoryInfo))
            {
                stopwatch ??= Stopwatch.StartNew();
                if (stopwatch.Elapsed > _options.CurrentValue.WaitLockTimeout)
                {
                    throw new InvalidOperationException(
                        "Waiting for lock to be removed has timed out");
                }

                // give some time to get rid of lock
                await Task.Delay(50);
            }
        }

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

        private bool TryDeleteDirectory(DirectoryInfo directoryInfo)
        {
            try
            {
                directoryInfo.Delete(recursive: true);
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }
    }
}
