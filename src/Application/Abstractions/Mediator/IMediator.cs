using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Abstractions.Mediator
{
    public interface IMediator
    {
        ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken)
            where TRequest : ICommand;

        ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : ICommand<TResult>;

        ValueTask Emit<TAggregate>(EventBase<TAggregate> @event, CancellationToken cancellationToken)
            where TAggregate : AggregateBase<TAggregate>;

        void Cancel<TAggregate>(long aggregateId)
            where TAggregate : AggregateBase<TAggregate>;
    }
}
