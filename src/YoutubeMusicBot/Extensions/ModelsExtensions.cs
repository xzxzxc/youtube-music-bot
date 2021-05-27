using Telegram.Bot.Types.ReplyMarkups;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
    public static class ModelsExtensions
    {
        public static InlineKeyboardMarkup ToMarkup(this InlineButton button) =>
            new(new InlineKeyboardButton
            {
                Text = button.Text, CallbackData = button.CallbackData,
            });


        public static InlineButton ToButton(this InlineKeyboardButton button) =>
            new(button.Text, button.CallbackData);
    }
}
