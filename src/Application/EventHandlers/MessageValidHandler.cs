using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class MessageValidHandler : IEventHandler<MessageValidEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly ICallbackDataFactory _callbackDataFactory;
        private readonly IRepository<Message> _repository;

        public MessageValidHandler(
            ITgClient tgClient,
            ICallbackDataFactory callbackDataFactory,
            IRepository<Message> repository)
        {
            _tgClient = tgClient;
            _callbackDataFactory = callbackDataFactory;
            _repository = repository;
        }

        public async ValueTask Handle(
            MessageValidEvent @event,
            CancellationToken cancellationToken = default)
        {
            var aggregate = @event.Aggregate;
            var messageId = await _tgClient.SendMessageAsync(
                aggregate.ChatId,
                "Loading started.",
                    new InlineButton(
                        "Cancel",
                        _callbackDataFactory.CreateForCancel(@event))
                        .ToCollection(),
                cancellationToken);
            aggregate.LoadingProcessMessageSent(messageId);
            await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
        }
    }
}
