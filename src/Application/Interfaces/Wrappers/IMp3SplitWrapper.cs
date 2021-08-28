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

    public interface IMusicSplitter
    {
        IAsyncEnumerable<string> SplitAsync(
            string filePath,
            IReadOnlyList<TrackModel> tracks,
            CancellationToken cancellationToken);

        IAsyncEnumerable<string> SplitBySilenceAsync(
            string filePath,
            CancellationToken cancellationToken);

        IAsyncEnumerable<string> SplitInEqualPartsAsync(
            string filePath,
            int partsCount,
            CancellationToken cancellationToken);
    }
}
