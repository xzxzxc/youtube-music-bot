using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

[assembly: InternalsVisibleTo("YoutubeMusicBot.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace YoutubeMusicBot
{
	internal class BotHostedService : IHostedService
	{
		private readonly ITelegramBotClient _client;
		private readonly ILogger _logger;
		private readonly IMediator _mediator;

		public BotHostedService(
			ITelegramBotClient client,
			ILogger<BotHostedService> logger,
			IMediator mediator)
		{
			_client = client;
			_logger = logger;
			_mediator = mediator;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_client.OnMessage += ProcessClientMessageAsync;
			_client.StartReceiving(
				allowedUpdates: new[] { UpdateType.Message },
				cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_client.StopReceiving();
			_client.OnMessage -= ProcessClientMessageAsync;
		}

		private async void ProcessClientMessageAsync(
			object? _,
			MessageEventArgs? messageEvent)
		{
			await _mediator.Publish(new MessageHandler.Message(messageEvent?.Message));

			// TODO: move logging to mediator pipeline

			//using var __ = _logger.BeginScope(
			//	"Message in chat with id: {Id}. Text: {text}.",
			//	messageEvent.Message.Chat.Id,
			//	messageEvent.Message.Text);

			//try
			//{
			//	await ProcessClientMessageAsync(messageEvent);
			//}
			//catch (Exception ex)
			//{
			//	_logger.LogError(
			//		ex,
			//		$"Exception during {nameof(ProcessClientMessageAsync)}");
			//}
		}
	}
}
