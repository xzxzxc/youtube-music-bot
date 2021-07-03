using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
	internal interface IMp3SplitWrapper
	{
		Task SplitAsync(
			FileInfo file,
			IReadOnlyCollection<TrackModel> tracks,
			CancellationToken cancellationToken = default);
	}
}
