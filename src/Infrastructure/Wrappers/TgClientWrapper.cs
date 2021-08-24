using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Infrastructure.Extensions;

namespace YoutubeMusicBot.Infrastructure.Wrappers
{
    public class TgClientWrapper : ITgClientWrapper,
        ITgClient
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly MessageContext _messageContext;

        public TgClientWrapper(
            MessageContext messageContext,
            ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _messageContext = messageContext;
        }

        private long ChatId => _messageContext.UserMessage.Chat.Id;

        private MessageModel? MessageToUpdate => _messageContext.MessageToUpdate;

        public Task<MessageModel> SendMessageAsync(
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            SendMessageAsync(
                ChatId,
                text,
                inlineButtons,
                cancellationToken);

        public Task<MessageModel> SendAudioAsync(
            IFileInfo audio,
            CancellationToken cancellationToken = default) =>
            SendAudioAsync(
                ChatId,
                audio,
                cancellationToken);

        public Task<MessageModel> UpdateMessageAsync(
            string text,
            CancellationToken cancellationToken = default) =>
            UpdateMessageAsync(
                ChatId,
                MessageToUpdate?.Id
                ?? throw new InvalidOperationException("There is no message to update"),
                text,
                MessageToUpdate.InlineButton?.ToCollection(),
                cancellationToken);

        public Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default) =>
            DeleteMessageAsync(
                ChatId,
                messageId,
                cancellationToken);

        public async Task<MessageModel> SendMessageAsync(
            long chatId,
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
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
            return await Ivoke(
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
            await Ivoke(
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
            await Ivoke(
                () => _telegramBotClient.DeleteMessageAsync(
                    chatId,
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
