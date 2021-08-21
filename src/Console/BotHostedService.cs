using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace YoutubeMusicBot.Console
{
    public class BotHostedService : BackgroundService
    {
        private readonly BotUpdatesProcessor _processor;

        public BotHostedService(
            BotUpdatesProcessor processor)
        {
            _processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _processor.ProcessUpdatesAsync(cancellationToken);
            }
        }
    }
}
