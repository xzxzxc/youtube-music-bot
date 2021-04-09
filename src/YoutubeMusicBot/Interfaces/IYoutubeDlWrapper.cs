using System.Threading;
using System.Threading.Tasks;

namespace YoutubeMusicBot
{
	internal interface IYoutubeDlWrapper
	{
		Task DownloadAsync(
			string folderPath,
			string url,
			CancellationToken cancellationToken = default);
	}
}
