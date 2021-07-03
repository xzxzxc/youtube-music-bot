using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
    public interface ITgClientWrapper
    {
        Task<MessageContext> SendMessageAsync(
            string text,
            CancellationToken cancellationToken = default);

        Task<MessageContext> SendMessageAsync(
            string text,
            InlineButton inlineButton,
            CancellationToken cancellationToken = default);

        Task<MessageContext> SendMessageAsync(
            string text,
            InlineButtonCollection inlineButtons,
            CancellationToken cancellationToken = default);

        Task<MessageContext> SendAudioAsync(
            FileInfo audio,
            CancellationToken cancellationToken = default);

        Task<MessageContext> UpdateMessageAsync(
            int messageId,
            string text,
            InlineButton? inlineButton = null,
            CancellationToken cancellationToken = default);

        Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
