using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using YoutubeMusicBot.Infrastructure.Abstractions;

namespace YoutubeMusicBot.Infrastructure
{
    public class YoutubeDlConfigPath : IYoutubeDlConfigPath, IHostedService
    {
        private readonly ILinuxPathResolver _linuxPathResolver;
        private string? _cachedValue;

        public YoutubeDlConfigPath(ILinuxPathResolver linuxPathResolver)
        {
            _linuxPathResolver = linuxPathResolver;
        }

        public string Value =>
            _cachedValue ?? throw new InvalidOperationException(
                $"Please call {nameof(StartAsync)} first");

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cachedValue = await _linuxPathResolver.Resolve(
                Path.Join(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "youtube-dl.conf"),
                cancellationToken: default);
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
