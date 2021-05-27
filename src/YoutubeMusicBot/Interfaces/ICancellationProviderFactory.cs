using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    internal interface ICancellationProviderFactory
    {
        CancellationProvider Create();
    }
}