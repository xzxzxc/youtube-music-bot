using System;

namespace YoutubeMusicBot.Application.Options
{
    // TODO: move to infrastructure on new architecture
    public class FileSystemOptions
    {
        public string TempFolderPath { get; set; } = "cache";

        public TimeSpan WaitLockTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
