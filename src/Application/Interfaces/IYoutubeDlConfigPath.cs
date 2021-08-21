using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IYoutubeDlConfigPath
    {
        ValueTask<string> GetValueAsync(CancellationToken cancellationToken);
    }
}
