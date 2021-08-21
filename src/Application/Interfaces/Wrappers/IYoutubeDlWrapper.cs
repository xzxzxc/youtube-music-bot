using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Console.Interfaces;

namespace YoutubeMusicBot.Console.Wrappers.Interfaces
{
    public interface IYoutubeDlWrapper
    {
        IAsyncEnumerable<IFileInfo> DownloadAsync(
            string url,
            CancellationToken cancellationToken = default);
    }
}
