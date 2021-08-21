using System.Collections.Generic;
using System.Threading;

namespace YoutubeMusicBot.Application.Interfaces.Wrappers
{
    public interface IYoutubeDlWrapper
    {
        IAsyncEnumerable<IFileInfo> DownloadAsync(
            string url,
            CancellationToken cancellationToken = default);
    }
}
