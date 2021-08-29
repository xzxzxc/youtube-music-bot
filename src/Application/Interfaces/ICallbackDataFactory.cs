using System;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface ICallbackDataFactory
    {
        ICallbackResult Parse(string callbackData);

        string CreateForCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>;
    }

    // TODO: move to folder
    public interface ICallbackResult
    {
    }

    public interface ICancelResult : ICallbackResult
    {
        long AggregateId { get; }

        Type AggregateType { get; }
    }

    public record CancelResult<TAggregate>(long AggregateId) : ICancelResult
        where TAggregate : AggregateBase<TAggregate>
    {
        public Type AggregateType { get; } = typeof(TAggregate);
    }
}
