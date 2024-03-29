﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Services
{
    public class EventSourcingRepository<TAggregate> : IRepository<TAggregate>
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
    }
}
