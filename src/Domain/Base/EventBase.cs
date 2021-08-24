using System.Threading;

namespace YoutubeMusicBot.Domain.Base
{
    public abstract record EventBase<TAggregate>
        where TAggregate : AggregateBase<TAggregate>
    {
        private static long _currentSequenceId = long.MinValue;
        private long? _id;

        public static void Initialize(long minId) =>
            _currentSequenceId = minId;

        public long Id
        {
            get => _id ??= Interlocked.Increment(ref _currentSequenceId);
            private set => _id = value;
        }

        public long AggregateId { get; init; }

        public TAggregate Aggregate { get; init; } = null!;
    }
}
