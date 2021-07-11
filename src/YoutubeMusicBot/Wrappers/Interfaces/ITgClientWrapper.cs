using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
    public interface ITgClientWrapper
    {
        Task<MessageModel> SendMessageAsync(
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default);

        Task<MessageModel> SendAudioAsync(
            FileInfo audio,
            CancellationToken cancellationToken = default);

        Task<MessageModel> UpdateMessageAsync(
            int messageId,
            string text,
            InlineButton? inlineButton = null,
            CancellationToken cancellationToken = default);

        Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
