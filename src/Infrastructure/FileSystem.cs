using System.IO;
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

        public string CreateTempFolder<T>(T folderId)
        {
            var folderPath = Path.Join(_options.CurrentValue.TempFolderPath, $"{folderId}");
            var folder = new DirectoryInfo(folderPath);
            folder.Create();

            return folder.FullName;
        }
    }
}
