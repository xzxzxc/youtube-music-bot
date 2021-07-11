using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Extensions
{
    public static class TgClientWrapperExtensions
    {
        public static Task<MessageModel> SendMessageAsync(
            this ITgClientWrapper tgClientWrapper,
            string text,
            InlineButton inlineButton,
            CancellationToken cancellationToken = default) =>
            tgClientWrapper.SendMessageAsync(
                text,
                new InlineButtonCollection(inlineButton),
                cancellationToken);
    }
}
