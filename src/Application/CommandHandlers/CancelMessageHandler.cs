using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.CommandHandlers
{
    public class CancelMessageHandler : ICommandHandler<CancelMessageHandler.Command>
    {
        private readonly IMediator _mediator;
        private readonly IRepository<Message> _repository;

        public CancelMessageHandler(
            IMediator mediator,
            IRepository<Message> repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        public async ValueTask Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            _mediator.Cancel<Message>(command.MessageId);

            var message = await _repository.GetByIdAsync(command.MessageId, cancellationToken);
            message.Cancalled();
            await _repository.SaveAndEmitEventsAsync(message, cancellationToken);
        }

        public record Command(long MessageId) : ICommand;
    }
}
