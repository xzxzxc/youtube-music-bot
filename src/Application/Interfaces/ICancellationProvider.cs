using System;
using System.Threading;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface ICancellationProvider : IDisposable
    {
        string CallbackData { get; }

        CancellationToken Token { get; }

        void Cancel();
    }
}