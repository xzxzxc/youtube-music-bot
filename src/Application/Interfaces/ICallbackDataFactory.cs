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

    public record CancelResult(string EventCancellationId) : ICallbackResult;
}
