using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class MessageFinishedHandler : IEventHandler<MessageFinishedEvent, Message>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ITgClient _tgClient;

        public MessageFinishedHandler(
            IFileSystem fileSystem,
            ITgClient tgClient)
        {
            _fileSystem = fileSystem;
            _tgClient = tgClient;
        }

        public async ValueTask Handle(
            MessageFinishedEvent @event,
            CancellationToken cancellationToken = default)
        {
            if (@event.Aggregate.ProcessMessageId.HasValue)
                await _tgClient.DeleteMessageAsync(
                    @event.Aggregate.ChatId,
                    @event.Aggregate.ProcessMessageId.Value,
                    cancellationToken);

            await _fileSystem.RemoveTempFolderAndContent(@event.AggregateId);
        }
    }
}
