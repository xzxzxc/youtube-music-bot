using System;
using System.IO;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Services
{
    public class DescriptionService : IDescriptionService
    {
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
