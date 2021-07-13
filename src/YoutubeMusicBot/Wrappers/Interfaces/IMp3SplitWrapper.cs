using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
    public interface IMp3SplitWrapper
    {
        Task SplitAsync(
            IFileInfo file,
            IReadOnlyCollection<TrackModel> tracks,
            CancellationToken cancellationToken = default);
    }
}
