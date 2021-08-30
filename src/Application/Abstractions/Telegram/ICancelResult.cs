using System;

namespace YoutubeMusicBot.Application.Abstractions.Telegram
{
    public interface ICancelResult : ICallbackResult
    {
        long AggregateId { get; }

        Type AggregateType { get; }
    }
}