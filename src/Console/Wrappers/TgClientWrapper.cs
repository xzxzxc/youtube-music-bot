using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YoutubeMusicBot.Console.Extensions;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Wrappers.Interfaces;

namespace YoutubeMusicBot.Console.Wrappers
{
    public class TgClientWrapper : ITgClientWrapper
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly long _chatId;

        public TgClientWrapper(
            MessageContext context,
            ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _chatId = context.UserMessage.Chat.Id;
        }

        public async Task<MessageModel> SendMessageAsync(
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.SendTextMessageAsync(
                    _chatId,
                    text,
                    replyMarkup: inlineButtons?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task<MessageModel> SendAudioAsync(
            IFileInfo audio,
            CancellationToken cancellationToken = default)
        {
            await using var fileStream = audio.OpenRead();
            var inputMedia = new InputMedia(
                fileStream,
                audio.Name);
            return await Ivoke(
                () => _telegramBotClient.SendAudioAsync(
                    _chatId,
                    inputMedia,
                    cancellationToken: cancellationToken));
        }

        public async Task<MessageModel> UpdateMessageAsync(
            int messageId,
            string text,
            InlineButton? inlineButton = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.EditMessageTextAsync(
                    _chatId,
                    messageId,
                    text,
                    replyMarkup: inlineButton?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.DeleteMessageAsync(
                    _chatId,
                    messageId,
                    cancellationToken));

        private async Task<MessageModel> Ivoke(Func<Task<Message>> action)
        {
            try
            {
                return (await action()).ToModel();
            }
            catch (ApiRequestException ex)
                when (ex.Parameters?.RetryAfter != null)
            {
                var time = TimeSpan.FromSeconds(ex.Parameters.RetryAfter);
                await Task.Delay(time);
                return await Ivoke(action);
            }
        }

        private async Task Ivoke(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (ApiRequestException ex)
                when (ex.Parameters?.RetryAfter != null)
            {
                var time = TimeSpan.FromSeconds(ex.Parameters.RetryAfter);
                await Task.Delay(time);
                await Ivoke(action);
            }
        }
    }
}