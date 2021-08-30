using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Infrastructure.Abstractions
{
    public interface ILinuxPathResolver
    {
        Task<string> Resolve(
            string currentOsPath,
            CancellationToken cancellationToken);
    }
}
