using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.EventSourcing
{
    public class EventSourcingRepository<TAggregate> :
        IRepository<TAggregate>,
        IInitializable
        where TAggregate : AggregateBase<TAggregate>
    {
        private readonly IDbContext _dbContext;
        private readonly IMediator _mediator;

        public EventSourcingRepository(
            IDbContext dbContext,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<TAggregate> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            var aggregate = (TAggregate)Activator.CreateInstance(
                typeof(TAggregate),
                nonPublic: true)!;

            var events = _dbContext.GetEventDbSet<TAggregate>()
                .AsQueryable()
                .Where(e => e.AggregateId == id)
                .OrderBy(e => e.Id)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            await foreach (var @event in events)
            {
                aggregate.ApplyEvent(@event);
            }

            return aggregate;
        }

        public async Task SaveAndEmitEventsAsync(
            TAggregate aggregate,
            CancellationToken cancellationToken = default)
        {
            var eventDbSet = _dbContext.GetEventDbSet<TAggregate>();

            var uncommittedEvents = aggregate.GetUncommittedEvents().ToArray();
            aggregate.ClearUncommittedEvents();

            eventDbSet.AddRange(uncommittedEvents);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var emitTasks = uncommittedEvents
                .Select(e => _mediator.Emit(e, cancellationToken).AsTask());
            await Task.WhenAll(emitTasks);
        }

        public async ValueTask Initialize()
        {
            var maxEventId = await _dbContext.GetEventDbSet<TAggregate>()
                .AsQueryable()
                .MaxAsync(e => (long?)e.Id);
            var maxAggregateId = await _dbContext.GetEventDbSet<TAggregate>()
                .AsQueryable()
                .MaxAsync(e => (long?)e.AggregateId);

            if (maxEventId.HasValue)
                EventBase<TAggregate>.Initialize(maxEventId.Value);
            if (maxAggregateId.HasValue)
                AggregateBase<TAggregate>.Initialize(maxAggregateId.Value);
        }
    }
}
