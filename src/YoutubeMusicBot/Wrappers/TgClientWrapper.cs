using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
    internal class TgClientWrapper : ITgClientWrapper
    {
        private readonly MessageContext _context;
        private readonly ITelegramBotClient _telegramBotClient;

        public TgClientWrapper(
            MessageContext context,
            ITelegramBotClient telegramBotClient)
        {
            _context = context;
            _telegramBotClient = telegramBotClient;
        }

        public async Task<MessageContext> SendAudioAsync(
            FileInfo audio,
            CancellationToken cancellationToken = default)
        {
            await using var fileStream = audio.OpenRead();
            var inputMedia = new InputMedia(
                fileStream,
                audio.Name);
            return await Ivoke(
                () => _telegramBotClient.SendAudioAsync(
                    _context.Chat.Id,
                    inputMedia,
                    cancellationToken: cancellationToken));
        }

        public async Task<MessageContext> SendMessageAsync(
            string text,
            InlineButton? inlineButton = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.SendTextMessageAsync(
                    _context.Chat.Id,
                    text,
                    replyMarkup: inlineButton?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task<MessageContext> UpdateMessageAsync(
            int messageId,
            string text,
            InlineButton? inlineButton = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.EditMessageTextAsync(
                    _context.Chat.Id,
                    messageId,
                    text,
                    replyMarkup: inlineButton?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.DeleteMessageAsync(
                    _context.Chat.Id,
                    messageId,
                    cancellationToken: cancellationToken));

        private async Task<MessageContext> Ivoke(Func<Task<Message>> action)
        {
            try
            {
                return (await action()).ToContext();
            }
            catch (ApiRequestException ex)
                when (ex.Parameters?.RetryAfter != null)
            {
                var time = TimeSpan.FromSeconds(ex.Parameters.RetryAfter.Value);
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
                var time = TimeSpan.FromSeconds(ex.Parameters.RetryAfter.Value);
                await Task.Delay(time);
                await Ivoke(action);
            }
        }
    }
}
