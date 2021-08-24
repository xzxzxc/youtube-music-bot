using System.Threading;
using System.Threading.Tasks;
using Autofac;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Mediator.Implementation
{
    public class MediatorService : IMediator
    {
        private readonly ILifetimeScope _scope;

        public MediatorService(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async ValueTask Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<IRequestHandler<TRequest>>();
            await handler.Handle(request, cancellationToken);
        }

        public async ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResult>
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<IRequestHandler<TRequest, TResult>>();
            return await handler.Handle(request, cancellationToken);
        }

        public async Task Emit<TAggregate>(
            EventBase<TAggregate> @event,
            CancellationToken cancellationToken = default)
            where TAggregate : AggregateBase<TAggregate>
        {
            var handlerType = typeof(IEventHandler<,>).MakeGenericType(
                @event.GetType(),
                typeof(TAggregate));
            await using var scope = _scope.BeginLifetimeScope();
            dynamic handler = scope.Resolve(handlerType);
            await handler.Handle((dynamic)@event, cancellationToken);
        }

        public async Task Cancel(string eventCancellationId)
        {
            throw new System.NotImplementedException();
        }
    }
}
