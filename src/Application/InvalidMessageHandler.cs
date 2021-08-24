using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class InvalidMessageHandler : IEventHandler<MessageInvalidEvent, Message>
    {
        private readonly ITgClient _tgClient;

        public InvalidMessageHandler(ITgClient tgClient)
        {
            _tgClient = tgClient;
        }

        public async ValueTask Handle(
            MessageInvalidEvent notification,
            CancellationToken cancellationToken = default)
        {
            await _tgClient.SendMessageAsync(
                notification.Aggregate.ChatId,
                notification.ValidationMessage,
                cancellationToken: cancellationToken);
        }
    }
}
