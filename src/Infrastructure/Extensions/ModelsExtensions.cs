using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeMusicBot.Application.Models.Telegram;

namespace YoutubeMusicBot.Infrastructure.Extensions
{
    public static class ModelsExtensions
    {
        public static InlineKeyboardMarkup ToMarkup(this InlineButtonCollection buttons) =>
            new(buttons.InlineButtons
                .Select(
                    bs => bs
                        .Select(b => b.ToButton())));

        private static InlineKeyboardButton ToButton(this InlineButton button) =>
            new(button.Text)
            {
                CallbackData = button.CallbackData,
            };
    }
}
