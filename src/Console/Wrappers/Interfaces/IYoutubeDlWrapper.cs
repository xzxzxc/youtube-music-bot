using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Console.Wrappers.Interfaces
{
    public interface IYoutubeDlWrapper
	{
		Task DownloadAsync(
			string url,
			CancellationToken cancellationToken = default);
	}
}
