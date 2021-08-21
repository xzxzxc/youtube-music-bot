using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Wrappers.Interfaces
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
