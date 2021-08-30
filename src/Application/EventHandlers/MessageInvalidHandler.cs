using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class MessageInvalidHandler : IEventHandler<MessageInvalidEvent, Message>
    {
        private readonly ITgClient _tgClient;

        public MessageInvalidHandler(ITgClient tgClient)
        {
            _tgClient = tgClient;
        }

        public async ValueTask Handle(
            MessageInvalidEvent @event,
            CancellationToken cancellationToken = default)
        {
            await _tgClient.SendMessageAsync(
                @event.Aggregate.ChatId,
                @event.ValidationMessage,
                cancellationToken: cancellationToken);
        }
    }
}
