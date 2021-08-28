using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Mediator
{
    public interface IMediator
    {
        ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IRequest;

        ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResult>;

        ValueTask Emit<TAggregate>(EventBase<TAggregate> @event, CancellationToken cancellationToken)
            where TAggregate : AggregateBase<TAggregate>;

        void Cancel(string eventCancellationId);
    }
}
