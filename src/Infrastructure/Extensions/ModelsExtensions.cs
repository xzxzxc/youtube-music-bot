using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Infrastructure.Extensions
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
            new(button.Text, button.CallbackData);

        private static InlineKeyboardButton ToButton(this InlineButton button) =>
            new()
            {
                Text = button.Text,
                CallbackData = button.CallbackData,
            };

        public static ChatModel ToModel(this Chat? chat) =>
            chat == null
                ? throw new ArgumentNullException(nameof(chat))
                : new(chat.Id);

        public static MessageModel ToModel(this Message? message) =>
            message == null
                ? throw new ArgumentNullException(nameof(message))
                : new(
                    message.MessageId,
                    message.Chat.ToModel(),
                    message.Text ?? string.Empty,
                    message.ReplyMarkup?.InlineKeyboard.First().First().ToButton());

        public static CallbackQueryContext ToContext(this CallbackQuery? callbackQuery)
        {
            if (callbackQuery == null)
                throw new ArgumentNullException(nameof(callbackQuery));
            if (callbackQuery.Message == null)
                throw new ArgumentException("Message must be not empty.", nameof(callbackQuery));
            if (callbackQuery.Data == null)
                throw new ArgumentException("Data must be not empty.", nameof(callbackQuery));

            return new(
                callbackQuery.Message.Chat.ToModel(),
                callbackQuery.Data);
        }
    }
}
