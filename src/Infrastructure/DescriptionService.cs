using System;
using System.IO;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Infrastructure.Wrappers;

namespace YoutubeMusicBot.Infrastructure
{
    public class DescriptionService : IDescriptionService
    {
        // TODO: return string when files would be deleted from aggregate
        public IFileInfo? GetFileOrNull(IFileInfo file)
        {
            var directoryName = file.DirectoryName
                ?? throw new ArgumentOutOfRangeException(
                    nameof(file),
                    file,
                    "File has no dictionary ");

            var fileName = Path.ChangeExtension(file.Name, "description");
            var res = new FileInfoWrapper(directoryName, fileName);

            return res.Exists
                ? res
                : null;
        }
    }
}
