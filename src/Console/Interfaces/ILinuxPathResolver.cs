using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Console.Interfaces
{
    public interface ILinuxPathResolver
    {
        Task<string> Resolve(
            string currentOsPath,
            CancellationToken cancellationToken);
    }
}
