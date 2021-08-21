using System.Collections.Generic;
using System.Threading;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IProcessRunner
    {
        IAsyncEnumerable<ProcessRunner.Line> RunAsync(
            ProcessRunner.Request request,
            CancellationToken cancellationToken = default);
    }
}
