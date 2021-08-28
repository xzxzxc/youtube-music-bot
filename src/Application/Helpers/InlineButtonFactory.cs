using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Helpers
{
    public static class InlineButtonFactory
    {
        public static InlineButtonCollection CreateCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>
            => new(new InlineButton("Cancel", @event.GetCancellationId()));
    }
}
