﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Infrastructure.Extensions;

namespace YoutubeMusicBot.Infrastructure
{
    public class TgClient : ITgClient
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public TgClient(
            ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task<MessageModel> SendMessageAsync(
            long chatId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            await Invoke(
                () => _telegramBotClient.SendTextMessageAsync(
                    chatId,
                    text,
                    replyMarkup: inlineButtons?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task<MessageModel> SendAudioAsync(
            long chatId,
            IFileInfo audio,
            CancellationToken cancellationToken = default)
        {
            await using var fileStream = audio.OpenRead();
            var inputMedia = new InputMedia(
                fileStream,
                audio.Name);
            return await Invoke(
                () => _telegramBotClient.SendAudioAsync(
                    chatId,
                    inputMedia,
                    cancellationToken: cancellationToken));
        }

        public async Task<MessageModel> UpdateMessageAsync(
            long chatId,
            int messageId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            await Invoke(
                () => _telegramBotClient.EditMessageTextAsync(
                    chatId,
                    messageId,
                    text,
                    replyMarkup: inlineButtons?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task DeleteMessageAsync(
            long chatId,
            int messageId,
            CancellationToken cancellationToken = default) =>
            await Invoke(
                () => _telegramBotClient.DeleteMessageAsync(
                    chatId,
                    messageId,
                    cancellationToken));

        private async Task<MessageModel> Invoke(Func<Task<Message>> action)
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
                return await Invoke(action);
            }
        }

        private async Task Invoke(Func<Task> action)
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
                await Invoke(action);
            }
        }
    }
}
