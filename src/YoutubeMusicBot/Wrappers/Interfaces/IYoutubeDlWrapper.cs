using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
	internal interface IYoutubeDlWrapper
	{
		Task DownloadAsync(
			string url,
			CancellationToken cancellationToken = default);
	}
}
