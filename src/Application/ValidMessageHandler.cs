using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Helpers;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class ValidMessageHandler : IEventHandler<MessageValidEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly IRepository<Message> _repository;

        public ValidMessageHandler(
            ITgClient tgClient,
            IRepository<Message> repository)
        {
            _tgClient = tgClient;
            _repository = repository;
        }

        public async ValueTask Handle(
            MessageValidEvent @event,
            CancellationToken cancellationToken = default)
        {
            var aggregate = @event.Aggregate;
            var message = await _tgClient.SendMessageAsync(
                aggregate.ChatId,
                "Loading started.",
                InlineButtonFactory.CreateCancel(@event),
                cancellationToken);
            aggregate.LoadingProcessMessageSent(message.Id);
            await _repository.SaveAsync(aggregate, cancellationToken);
        }
    }
}
