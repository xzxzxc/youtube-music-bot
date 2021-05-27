using System;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
    public static class TgClientModelsExtensions
    {
        public static ChatContext ToContext(this Chat? chat) =>
            chat == null
                ? throw new ArgumentNullException(nameof(chat))
                : new(chat.Id);

        public static MessageContext ToContext(this Message? message) =>
            message == null
                ? throw new ArgumentNullException(nameof(message))
                : new(
                    message.MessageId,
                    message.Chat.ToContext(),
                    message.Text ?? string.Empty);

        public static CallbackQueryContext ToContext(this CallbackQuery? callbackQuery)
        {
            if (callbackQuery == null)
                throw new ArgumentNullException(nameof(callbackQuery));
            if (callbackQuery.Message == null)
                throw new ArgumentException("Message must be not empty.", nameof(callbackQuery));

            return new(
                callbackQuery.Message.Chat.ToContext(),
                callbackQuery.Data);
        }
    }
}
