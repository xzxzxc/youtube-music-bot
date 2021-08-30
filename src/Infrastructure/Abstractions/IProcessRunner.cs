using System.Collections.Generic;
using System.Threading;
using YoutubeMusicBot.Infrastructure.Models.ProcessRuner;

namespace YoutubeMusicBot.Infrastructure.Abstractions
{
    public interface IProcessRunner
    {
        IAsyncEnumerable<ProcessResultLine> RunAsync(
            ProcessOptions options,
            CancellationToken cancellationToken = default);
    }
}
