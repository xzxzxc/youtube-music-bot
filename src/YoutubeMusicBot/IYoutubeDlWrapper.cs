using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot
{
	internal interface IYoutubeDlWrapper
	{
		Task<IFileWrapper> DownloadAsync(
			string url,
			CancellationToken cancellationToken = default);
	}
}
