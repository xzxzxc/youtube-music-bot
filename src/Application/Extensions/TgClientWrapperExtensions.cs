using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Extensions
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
