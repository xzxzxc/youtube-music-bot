using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Handlers;

namespace YoutubeMusicBot
{
    public class BotHostedService : BackgroundService
    {
        private readonly ITelegramBotClient _client;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public BotHostedService(
            ITelegramBotClient client,
            IMediator mediator,
            ILogger<BotHostedService> logger)
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var timeout = Convert.ToInt32(Timeout.TotalSeconds);
                var updates = Array.Empty<Update>();

                try
                {
                    updates = await _client.GetUpdatesAsync(
                        MessageOffset,
                        timeout: timeout,
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
                catch (Exception ex) when (
                    ex is not OperationCanceledException and not TaskCanceledException)
                {
                    _logger.LogError(ex, "Exception during get or process update");
                }
            }
        }

        private async Task ProcessUpdateAsync(
            Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(
                    new UpdateHandler.Request(update),
                    cancellationToken);
            }
            catch
            {
                // exception is handled in mediatr
            }
        }
    }
}
