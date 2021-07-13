using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Wrappers;

namespace YoutubeMusicBot.Services
{
    public class YoutubeDlConfigPath : IYoutubeDlConfigPath
    {
        private readonly ILinuxPathResolver _linuxPathResolver;
        private string? _cachedValue;

        public YoutubeDlConfigPath(ILinuxPathResolver linuxPathResolver)
        {
            _linuxPathResolver = linuxPathResolver;
        }
        public async ValueTask<string> GetValueAsync(CancellationToken cancellationToken)
        {
            return _cachedValue ??= await _linuxPathResolver.Resolve(
                Path.Join(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "youtube-dl.conf"),
                cancellationToken);
        }
    }
}
