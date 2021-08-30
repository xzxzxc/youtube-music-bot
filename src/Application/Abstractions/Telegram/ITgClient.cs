using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Models.Telegram;

namespace YoutubeMusicBot.Application.Abstractions.Telegram
{
    public interface ITgClient
    {
        Task<int> SendMessageAsync(
            long chatId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default);

        Task<int> SendAudioAsync(
            long chatId,
            Stream fileReadStream,
            string title,
            CancellationToken cancellationToken = default);

        Task UpdateMessageAsync(
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
