using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Interfaces.Wrappers
{
    public interface ITgClient
    {
        Task<MessageModel> SendMessageAsync(
            long chatId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default);

        Task<MessageModel> SendAudioAsync(
            long chatId,
            IFileInfo audio,
            CancellationToken cancellationToken = default);

        Task<MessageModel> UpdateMessageAsync(
            long chatId,
            int messageId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default);

        Task DeleteMessageAsync(
            long chatId,
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
