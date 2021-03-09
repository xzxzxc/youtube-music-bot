using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly IYoutubeDlWrapper _youtubeDlWrapper;
		private readonly ILogger _logger;

		public BotHostedService(
			ITelegramBotClient client,
			IYoutubeDlWrapper youtubeDlWrapper,
			ILogger<BotHostedService> logger)
		{
			_client = client;
			_youtubeDlWrapper = youtubeDlWrapper;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_client.OnMessage += ProcessClientMessageAsync;
			_client.StartReceiving(
				allowedUpdates: new[] { UpdateType.Message },
				cancellationToken);
		}

		public async Task ProcessClientMessageAsync(
			MessageEventArgs messageEvent)
		{
			await using var file =
				await _youtubeDlWrapper.DownloadAsync(
					messageEvent.Message.Text);

			await using var fileStream = file.Stream;

			await _client.SendAudioAsync(
				messageEvent.Message.Chat.Id,
				new InputMedia(fileStream, file.Name));
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_client.StopReceiving();
			_client.OnMessage -= ProcessClientMessageAsync;
		}

		private async void ProcessClientMessageAsync(
			object? _,
			MessageEventArgs messageEvent)
		{
			using var __ = _logger.BeginScope(
				"Message in chat with id: {Id}. Text: {text}.",
				messageEvent.Message.Chat.Id,
				messageEvent.Message.Text);

			try
			{
				await ProcessClientMessageAsync(messageEvent);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					$"Exception during {nameof(ProcessClientMessageAsync)}");
			}
		}
	}
}
