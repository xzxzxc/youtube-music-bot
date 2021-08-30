using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Abstractions.Mediator
{
    public interface IEventHandler<TEvent, TAggregate>
        where TEvent : EventBase<TAggregate>
        where TAggregate : AggregateBase<TAggregate>
    {
        ValueTask Handle(TEvent @event, CancellationToken cancellationToken);
    }
}