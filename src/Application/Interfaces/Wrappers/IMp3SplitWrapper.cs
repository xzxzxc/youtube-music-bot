using System.Collections.Generic;
using System.Threading;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Interfaces.Wrappers
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
