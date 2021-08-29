using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class CancelMessageHandler : IRequestHandler<CancelMessageHandler.Request>
    {
        private readonly IMediator _mediator;
        private readonly ITgClient _tgClient;
        private readonly IRepository<Message> _repository;

        public CancelMessageHandler(
            IMediator mediator,
            ITgClient tgClient,
            IRepository<Message> repository)
        {
            _mediator = mediator;
            _tgClient = tgClient;
            _repository = repository;
        }

        public async ValueTask Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            _mediator.Cancel<Message>(request.MessageId);

            var message = await _repository.GetByIdAsync(request.MessageId, cancellationToken);
            message.Finished();
            await _repository.SaveAndEmitEventsAsync(message, cancellationToken);
        }

        public record Request(long MessageId) : IRequest;
    }
}
