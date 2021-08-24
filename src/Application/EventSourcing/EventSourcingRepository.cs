using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Options;
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
        private readonly IOptionsMonitor<FeatureOptions> _featureOptions;

        public EventSourcingRepository(
            IDbContext dbContext,
            IMediator mediator,
            IOptionsMonitor<FeatureOptions> featureOptions)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _featureOptions = featureOptions;
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

        public async Task SaveAsync(
            TAggregate aggregate,
            CancellationToken cancellationToken = default)
        {
            var eventDbSet = _dbContext.GetEventDbSet<TAggregate>();
            foreach (var @event in aggregate.GetUncommittedEvents())
            {
                eventDbSet.Add(@event);
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (_featureOptions.CurrentValue.EsArchitectureEnabled)
                {
                    await _mediator.Emit(@event, cancellationToken);
                }
            }

            aggregate.ClearUncommittedEvents();
        }

        public async ValueTask Initialize()
        {
            var maxEventId = await _dbContext.GetEventDbSet<TAggregate>()
                .AsQueryable()
                .MaxAsync(e => e.Id);
            var maxAggregateId = await _dbContext.GetEventDbSet<TAggregate>()
                .AsQueryable()
                .MaxAsync(e => e.AggregateId);

            EventBase<TAggregate>.Initialize(maxEventId);
            AggregateBase<TAggregate>.Initialize(maxAggregateId);
        }
    }
}
