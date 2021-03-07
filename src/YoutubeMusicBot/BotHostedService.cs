using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

[assembly: InternalsVisibleTo("YoutubeMusicBot.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace YoutubeMusicBot
{
	internal class BotHostedService : IHostedService
	{
		private readonly ITelegramBotClient _client;
		private readonly IYoutubeDlWrapper _youtubeDlWrapper;
		private readonly ILogger<BotHostedService> _logger;

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
			_client.OnMessage += ClientOnOnMessage;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_client.OnMessage -= ClientOnOnMessage;
		}

		private async void ClientOnOnMessage(
			object? _,
			MessageEventArgs messageEvent)
		{
			using var __ = _logger.BeginScope(
				"Message in chat with id: {Id}. Text: {text}.",
				messageEvent.Message.Chat.Id,
				messageEvent.Message.Text);

			try
			{
				await using var file =
					await _youtubeDlWrapper.DownloadAsync(
						messageEvent.Message.Text);

				await _client.SendAudioAsync(
					messageEvent.Message.Chat.Id,
					new InputMedia(file.Stream, file.Name));
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					$"Exception during {nameof(ClientOnOnMessage)}");
			}
		}
	}

	internal interface IYoutubeDlWrapper
	{
		Task<IFileWrapper> DownloadAsync(string url);
	}

	interface IFileWrapper : IAsyncDisposable
	{
		Stream Stream { get; }

		string Name { get; }
	}
}
