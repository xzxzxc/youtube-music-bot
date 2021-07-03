using System.Linq;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
    public static class ModelsExtensions
    {
        public static InlineKeyboardMarkup ToMarkup(this InlineButton button) =>
            new(button.ToButton());

        public static InlineKeyboardMarkup ToMarkup(this InlineButtonCollection buttons) =>
            new(buttons.InlineButtons
                .Select(
                    bs => bs
                        .Select(b => b.ToButton())));

        public static InlineButton ToButton(this InlineKeyboardButton button) =>
            new(button.Text, button.CallbackData == null
                ? null
                : Encoding.Unicode.GetBytes(button.CallbackData));

        private static InlineKeyboardButton ToButton(this InlineButton button) =>
            new()
            {
                Text = button.Text,
                CallbackData = button.CallbackData == null
                    ? null
                    : Encoding.Unicode.GetString(button.CallbackData),
            };
    }
}
