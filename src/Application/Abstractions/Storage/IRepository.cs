using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Abstractions.Storage
{
    public interface IRepository<TAggregate>
        where TAggregate : AggregateBase<TAggregate>
    {
        Task<TAggregate> GetByIdAsync(long id, CancellationToken cancellationToken);

        Task SaveAndEmitEventsAsync(TAggregate aggregate, CancellationToken cancellationToken);
    }
}
