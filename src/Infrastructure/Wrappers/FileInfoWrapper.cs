using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Interfaces.Wrappers;

namespace YoutubeMusicBot.Infrastructure.Wrappers
{
    public record FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo _file;

        public FileInfoWrapper(params string[] pathParts)
        {
            _file = new FileInfo(Path.Join(pathParts));
        }

        public bool Exists => _file.Exists;

        public string Name => _file.Name;

        public string Extension => _file.Extension;

        public string FullName => _file.FullName;

        public string DirectoryName => _file.DirectoryName ?? string.Empty;

        public long Length => _file.Length;

        public Task<string> GetTextAsync(CancellationToken cancellationToken = default) =>
            File.ReadAllTextAsync(_file.FullName, cancellationToken);

        public void Delete() =>
            _file.Delete();

        public Stream OpenRead() =>
            _file.OpenRead();
    }
}
