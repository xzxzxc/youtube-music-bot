using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using YoutubeMusicBot.Console.Handlers;

namespace YoutubeMusicBot.Console.Interfaces
{
    public interface IProcessRunner
    {
        IAsyncEnumerable<ProcessRunner.Line> RunAsync(
            ProcessRunner.Request request,
            CancellationToken cancellationToken = default);
    }
}
