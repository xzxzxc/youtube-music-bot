using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Extensions;

[assembly: InternalsVisibleTo("YoutubeMusicBot.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace YoutubeMusicBot
{
	internal class BotHostedService : IHostedService
	{
		private readonly ITelegramBotClient _client;
		private readonly IMediator _mediator;

		public BotHostedService(
			ITelegramBotClient client,
			IMediator mediator)
		{
			_client = client;
			_mediator = mediator;
		}

		/// <summary>
		/// The current message offset
		/// </summary>
		private int MessageOffset { get; set; }

		private TimeSpan Timeout => _client.Timeout;

		public UpdateType[] AllowedUpdates { get; } =
			{ UpdateType.Message };

		public async Task StartAsync(CancellationToken cancellationToken)
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
				}
				catch (OperationCanceledException)
				{
				}
				catch (ApiRequestException apiException)
				{
					// TODO:
					//OnReceiveError?.Invoke(this, apiException);
				}
				catch (Exception generalException)
				{
					// TODO:
					//OnReceiveGeneralError?.Invoke(this, generalException);
				}

				try
				{
					foreach (var update in updates)
					{
#pragma warning disable 4014
						ProcessMessageAsync(update);
#pragma warning restore 4014
						MessageOffset = update.Id + 1;
					}
				}
				catch
				{
					throw;
				}
			}
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
		}

		private async Task ProcessMessageAsync(
			Update update)
		{
			try
			{
				await _mediator.Send(
					new MessageHandler.Request(update.Message?.ToContext()));
			}
			catch
			{
			}
		}
	}
}
