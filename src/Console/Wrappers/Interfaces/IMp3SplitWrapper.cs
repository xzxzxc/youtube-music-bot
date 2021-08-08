using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Wrappers.Interfaces
{
    public interface IMp3SplitWrapper
    {
        Task SplitAsync(
            IFileInfo file,
            IReadOnlyCollection<TrackModel> tracks,
            CancellationToken cancellationToken = default);
    }
}
