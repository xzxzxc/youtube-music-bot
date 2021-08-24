using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class ValidMessageHandler : IEventHandler<MessageValidEvent, Message>
    {
        private readonly ITgClient _tgClientWrapper;

        public ValidMessageHandler(ITgClient tgClientWrapper)
        {
            _tgClientWrapper = tgClientWrapper;
        }

        public async ValueTask Handle(
            MessageValidEvent notification,
            CancellationToken cancellationToken = default)
        {
            await _tgClientWrapper.SendMessageAsync(
                notification.Aggregate.ChatId,
                "Loading started.",
                new(new InlineButton("Cancel", notification.GetCancellationId())),
                cancellationToken);

        }
    }
}
