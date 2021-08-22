using System;

namespace YoutubeMusicBot.Application.Options
{
    public class SplitOptions
    {
        public TimeSpan MinSilenceLength { get; set; } = TimeSpan.FromSeconds(0.5);
    }
}
