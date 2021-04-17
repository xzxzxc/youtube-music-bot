using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

[assembly: InternalsVisibleTo("YoutubeMusicBot.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace YoutubeMusicBot
{
	internal class BotHostedService : IHostedService
	{
		private readonly ITelegramBotClient _client;
		private readonly Func<IMediator> _mediatorFactory;

		public BotHostedService(
			ITelegramBotClient client,
			Func<IMediator> mediatorFactory)
		{
			_client = client;
			_mediatorFactory = mediatorFactory;
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
			await _mediatorFactory().Send(
				new MessageHandler.Request(messageEvent?.Message));
		}
	}
}
