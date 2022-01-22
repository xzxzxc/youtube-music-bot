using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.CommandHandlers;

namespace YoutubeMusicBot.Console
{
    public class BotUpdatesProcessor
    {
        private readonly ITelegramBotClient _client;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public BotUpdatesProcessor(
            ITelegramBotClient client,
            IMediator mediator,
            ILogger<BotUpdatesProcessor> logger)
        {
            _client = client;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// The current message offset
        /// </summary>
        private int MessageOffset { get; set; }

        private TimeSpan Timeout => _client.Timeout;

        private UpdateType[] AllowedUpdates { get; } =
        {
            UpdateType.Message, UpdateType.CallbackQuery
        };

        public async Task ProcessUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var updates = await _client.GetUpdatesAsync(
                    MessageOffset,
                    timeout: (int)Timeout.TotalSeconds,
                    allowedUpdates: AllowedUpdates,
                    cancellationToken: cancellationToken);

                foreach (var update in updates)
                {
                    try
                    {
#pragma warning disable 4014
                        ProcessUpdateAsync(update, cancellationToken);
#pragma warning restore 4014
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Exception during process update");
                    }

                    MessageOffset = update.Id + 1;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during get update");
            }
        }

        private async Task ProcessUpdateAsync(
            Update update,
            CancellationToken cancellationToken) =>
            await (update switch
            {
                { Message : { } } => _mediator.Send(
                    new MessageHandler.Command(
                        update.Message.MessageId,
                        update.Message.Chat.Id,
                        update.Message.Text ?? string.Empty),
                    cancellationToken),
                { CallbackQuery : { } } => _mediator.Send(
                    new CallbackQueryHandler.Command(update.CallbackQuery.Data),
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(update), update, "Unknown update")
            });
    }
}
