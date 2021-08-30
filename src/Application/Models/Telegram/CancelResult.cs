using System;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Models.Telegram
{
    public record CancelResult<TAggregate>(long AggregateId) : ICancelResult
        where TAggregate : AggregateBase<TAggregate>
    {
        public Type AggregateType { get; } = typeof(TAggregate);
    }
}