using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Abstractions.Mediator
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        ValueTask Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        ValueTask<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
}
