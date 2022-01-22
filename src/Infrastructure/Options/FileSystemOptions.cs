using System;

namespace YoutubeMusicBot.Infrastructure.Options
{
    public class FileSystemOptions
    {
        public string TempFolderPath { get; set; } = "cache";

        public TimeSpan WaitLockTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }
}
