using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Services;

public class RepositoryInitializer<TAggregate> : IHostedService
    where TAggregate : AggregateBase<TAggregate>
{
    private readonly IDbContext _dbContext;

    public RepositoryInitializer(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var maxEventId = await _dbContext.GetEventDbSet<TAggregate>()
            .AsQueryable()
            .MaxAsync(e => (long?)e.Id, cancellationToken);
        var maxAggregateId = await _dbContext.GetEventDbSet<TAggregate>()
            .AsQueryable()
            .MaxAsync(e => (long?)e.AggregateId, cancellationToken);

        if (maxEventId.HasValue)
            EventBase<TAggregate>.Initialize(maxEventId.Value);
        if (maxAggregateId.HasValue)
            AggregateBase<TAggregate>.Initialize(maxAggregateId.Value);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
