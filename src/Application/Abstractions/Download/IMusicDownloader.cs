using System.Collections.Generic;
using System.Threading;

namespace YoutubeMusicBot.Application.Abstractions.Download
{
    public interface IMusicDownloader
    {
        IAsyncEnumerable<IDownloadResult> DownloadAsync(
            string pathToDownloadTo,
            string url,
            CancellationToken cancellationToken = default);
    }
}
