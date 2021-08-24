using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Extensions
{
    public static class EventExtensions
    {
        public static string GetCancellationId<TAggregate>(this EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate> =>
            $"{typeof(TAggregate).Name.GetHashCode() ^ @event.Id}";
    }
}
