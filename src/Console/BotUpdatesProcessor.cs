using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Infrastructure.Extensions;

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
#pragma warning disable 4014
                    ProcessUpdateAsync(update, cancellationToken);
#pragma warning restore 4014
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
            CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await _mediator.Send(
                            new MessageHandler.Request(update.Message.ToModel()),
                            cancellationToken);
                        break;
                    case UpdateType.CallbackQuery:
                        await _mediator.Send(
                            new CallbackQueryHandler.Request(update.CallbackQuery.ToContext()),
                            cancellationToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Exception during process update");
            }
        }
    }
}
