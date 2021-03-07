using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

[assembly: InternalsVisibleTo("YoutubeMusicBot.Tests")]

namespace YoutubeMusicBot
{
	internal class BotHostedService : IHostedService
	{
		private readonly ITelegramBotClient _client;

		public BotHostedService(
			ITelegramBotClient client)
		{
			_client = client;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			throw new System.NotImplementedException();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			throw new System.NotImplementedException();
		}
	}
}
