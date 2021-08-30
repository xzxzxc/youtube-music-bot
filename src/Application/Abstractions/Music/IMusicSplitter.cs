using System.Collections.Generic;
using System.Threading;
using YoutubeMusicBot.Application.Models.Music;

namespace YoutubeMusicBot.Application.Abstractions.Music
{
    public interface IMusicSplitter
    {
        IAsyncEnumerable<string> SplitAsync(
            string filePath,
            IReadOnlyList<Track> tracks,
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
