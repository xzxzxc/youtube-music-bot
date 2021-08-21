using System.Collections.Generic;
using System.Threading;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Wrappers.Interfaces
{
    public interface IMp3SplitWrapper
    {
        IAsyncEnumerable<IFileInfo> SplitAsync(
            IFileInfo file,
            TracksList tracks,
            CancellationToken cancellationToken);

        IAsyncEnumerable<IFileInfo> SplitBySilenceAsync(
            IFileInfo file,
            CancellationToken cancellationToken);

        IAsyncEnumerable<IFileInfo> SplitInEqualPartsAsync(
            IFileInfo file,
            int partsCount,
            CancellationToken cancellationToken);
    }
}
