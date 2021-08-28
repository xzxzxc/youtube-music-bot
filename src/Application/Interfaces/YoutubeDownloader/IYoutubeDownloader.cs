using System.Collections.Generic;
using System.Threading;

namespace YoutubeMusicBot.Application.Interfaces.YoutubeDownloader
{
    public interface IYoutubeDownloader
    {
        IAsyncEnumerable<IDownloadResult> DownloadAsync(
            string pathToDownloadTo,
            string url,
            CancellationToken cancellationToken = default);
    }
}
