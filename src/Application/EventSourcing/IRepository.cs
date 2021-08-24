using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.EventSourcing
{
    public interface IRepository<TAggregate>
        where TAggregate : AggregateBase<TAggregate>
    {
        Task<TAggregate> GetByIdAsync(long id, CancellationToken cancellationToken);

        Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken);
    }
}
