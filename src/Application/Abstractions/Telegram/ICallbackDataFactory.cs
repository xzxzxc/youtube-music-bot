using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Abstractions.Telegram
{
    public interface ICallbackDataFactory
    {
        ICallbackResult Parse(string callbackData);

        string CreateForCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>;
    }
}
