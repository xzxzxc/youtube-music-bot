using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Interfaces
{
    public interface IYoutubeDlConfigPath
    {
        ValueTask<string> GetValueAsync(CancellationToken cancellationToken);
    }
}
