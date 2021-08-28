using System;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Services
{
    public class CallbackDataFactory : ICallbackDataFactory
    {
        public ICallbackResult Parse(string callbackData) =>
            callbackData[0] switch
            {
                'c' => new CancelResult(callbackData),
                _ => throw new InvalidOperationException(
                    $"Unknown callback data identifier {callbackData[0]}")
            };

        public string CreateForCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate> =>
            $"c{typeof(TAggregate).Name.GetHashCode() ^ @event.Id}";
    }
}
