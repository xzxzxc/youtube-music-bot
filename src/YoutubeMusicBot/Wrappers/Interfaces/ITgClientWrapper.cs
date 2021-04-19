using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
	public interface ITgClientWrapper
	{
		Task<Message> SendAudioAsync(
			FileInfo audio,
			CancellationToken cancellationToken = default);
	}
}
