using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public interface IDbContext
    {
        DbSet<Message> Messages { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
