using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    public interface ICancellationRegistration
    {
        CancellationProvider GetProvider(string cancellationProviderId);

        CancellationProvider RegisterNewProvider();
    }
}