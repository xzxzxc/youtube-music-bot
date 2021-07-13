using System.IO;
using YoutubeMusicBot.Interfaces;

namespace YoutubeMusicBot.Models
{
    public class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo _file;

        public FileInfoWrapper(params string[] pathParts)
        {
            _file = new FileInfo(Path.Join(pathParts));
        }

        public bool Exists => _file.Exists;

        public string Name => _file.Name;

        public string DirectoryName => _file.DirectoryName ?? string.Empty;

        public long Length => _file.Length;

        public void Delete() =>
            _file.Delete();

        public Stream OpenRead() =>
            _file.OpenRead();
    }
}
