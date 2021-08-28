using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Helpers;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class ValidMessageHandler : IEventHandler<MessageValidEvent, Message>
    {
        private readonly ITgClient _tgClient;

        public ValidMessageHandler(
            ITgClient tgClient)
        {
            _tgClient = tgClient;
        }

        public async ValueTask Handle(
            MessageValidEvent @event,
            CancellationToken cancellationToken = default)
        {
            var message = await _tgClient.SendMessageAsync(
                @event.Aggregate.ChatId,
                "Loading started.",
                InlineButtonFactory.CreateCancel(@event),
                cancellationToken);
            @event.Aggregate.LoadingProcessMessageSent(message.Id);
        }
    }
}
