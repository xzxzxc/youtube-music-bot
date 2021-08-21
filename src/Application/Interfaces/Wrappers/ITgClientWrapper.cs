using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Interfaces.Wrappers
{
    public interface ITgClientWrapper
    {
        Task<MessageModel> SendMessageAsync(
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default);

        Task<MessageModel> SendAudioAsync(
            IFileInfo audio,
            CancellationToken cancellationToken = default);

        Task<MessageModel> UpdateMessageAsync(
            string text,
            CancellationToken cancellationToken = default);

        Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
