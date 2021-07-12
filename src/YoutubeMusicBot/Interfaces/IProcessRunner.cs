using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using YoutubeMusicBot.Handlers;

namespace YoutubeMusicBot.Interfaces
{
    public interface IProcessRunner
    {
        IAsyncEnumerable<ProcessRunner.Line> RunAsync(
            ProcessRunner.Request request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
