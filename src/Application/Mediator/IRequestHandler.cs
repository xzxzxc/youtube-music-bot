using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Mediator
{
    public interface IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        ValueTask Handle(TRequest @event, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        ValueTask<TResult> Handle(TRequest @event, CancellationToken cancellationToken);
    }
}
