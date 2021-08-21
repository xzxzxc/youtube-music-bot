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

        public async Task<MessageModel> SendMessageAsync(
            string text,
            InlineButtonCollection? inlineButtons = null,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.SendTextMessageAsync(
                    ChatId,
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
                    ChatId,
                    inputMedia,
                    cancellationToken: cancellationToken));
        }

        public async Task<MessageModel> UpdateMessageAsync(
            string text,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.EditMessageTextAsync(
                    ChatId,
                    MessageToUpdate?.Id
                    ?? throw new InvalidOperationException("There is no message to update"),
                    text,
                    replyMarkup: MessageToUpdate.InlineButton?.ToMarkup(),
                    cancellationToken: cancellationToken));

        public async Task DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default) =>
            await Ivoke(
                () => _telegramBotClient.DeleteMessageAsync(
                    ChatId,
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
