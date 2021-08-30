using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Infrastructure.Abstractions;

namespace YoutubeMusicBot.Infrastructure
{
    public class YoutubeDlConfigPath : IYoutubeDlConfigPath, IInitializable
    {
        private readonly ILinuxPathResolver _linuxPathResolver;
        private string? _cachedValue;

        public YoutubeDlConfigPath(ILinuxPathResolver linuxPathResolver)
        {
            _linuxPathResolver = linuxPathResolver;
        }

        public string Value =>
            _cachedValue ?? throw new InvalidOperationException(
                $"Please call {nameof(Initialize)} first");

        public async ValueTask Initialize()
        {
            _cachedValue = await _linuxPathResolver.Resolve(
                Path.Join(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "youtube-dl.conf"),
                cancellationToken: default);
        }
    }
}
