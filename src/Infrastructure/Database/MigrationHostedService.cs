using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace YoutubeMusicBot.Infrastructure.Database;

public class MigrationHostedService : IHostedService
{
    private readonly ApplicationDbContext _dbContext;

    public MigrationHostedService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
