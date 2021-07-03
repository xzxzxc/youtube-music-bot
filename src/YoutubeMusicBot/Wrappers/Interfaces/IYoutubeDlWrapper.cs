using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
    public interface IYoutubeDlWrapper
	{
		Task DownloadAsync(
			string url,
			CancellationToken cancellationToken = default);
	}
}
