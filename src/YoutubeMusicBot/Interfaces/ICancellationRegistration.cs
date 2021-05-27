using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    internal interface ICancellationRegistration
    {
        CancellationProvider GetProvider(string cancellationProviderId);

        CancellationProvider RegisterNewProvider();
    }
}