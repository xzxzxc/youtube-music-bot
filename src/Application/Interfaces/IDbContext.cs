using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IDbContext
    {
        DbSet<T> GetDbSet<T>()
            where T : class;

        DbSet<EventBase<TAggregate>> GetEventDbSet<TAggregate>()
            where TAggregate : AggregateBase<TAggregate> =>
            GetDbSet<EventBase<TAggregate>>();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
