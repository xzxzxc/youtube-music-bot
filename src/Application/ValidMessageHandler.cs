using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class ValidMessageHandler : IEventHandler<MessageValidEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly ICallbackDataFactory _callbackDataFactory;
        private readonly IRepository<Message> _repository;

        public ValidMessageHandler(
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
            var message = await _tgClient.SendMessageAsync(
                aggregate.ChatId,
                "Loading started.",
                new InlineButtonCollection(
                    new InlineButton(
                        "Cancel",
                        _callbackDataFactory.CreateForCancel(@event))),
                cancellationToken);
            aggregate.LoadingProcessMessageSent(message.Id);
            await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
        }
    }
}
