using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YoutubeMusicBot.Domain.Base
{
    public abstract class AggregateBase<TAggregate>
        where TAggregate : AggregateBase<TAggregate>
    {
        private static long _currentSequenceId = long.MinValue;

        public static void Initialize(long minId) =>
            _currentSequenceId = minId;

        private readonly List<EventBase<TAggregate>> _uncommittedEvents = new();
        private long? _id;

        public long Id
        {
            get => _id ??= Interlocked.Increment(ref _currentSequenceId);
            protected set => _id = value;
        }

        protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : EventBase<TAggregate>
        {
            var eventWithAggregate = @event with
            {
                AggregateId = Id,
                Aggregate = (TAggregate)this,
            };

            ApplyEvent(eventWithAggregate);
            _uncommittedEvents.Add(eventWithAggregate);
        }

        public void ApplyEvent(EventBase<TAggregate> @event)
        {
            if (_uncommittedEvents.Any(x => x.Id == @event.Id))
                return;

            ((dynamic)this).Apply((dynamic)@event);
        }

        public void ClearUncommittedEvents() =>
            _uncommittedEvents.Clear();

        public IEnumerable<EventBase<TAggregate>> GetUncommittedEvents() =>
            _uncommittedEvents.AsEnumerable();
    }
}
