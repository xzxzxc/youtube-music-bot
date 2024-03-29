﻿using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class FileToBeSentCreatedHandler : IEventHandler<FileToBeSentCreatedEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly IFileSystem _fileSystem;

        public FileToBeSentCreatedHandler(
            ITgClient tgClient,
            IFileSystem fileSystem)
        {
            _tgClient = tgClient;
            _fileSystem = fileSystem;
        }

        public async ValueTask Handle(
            FileToBeSentCreatedEvent @event,
            CancellationToken cancellationToken = default)
        {
            var readStream = _fileSystem.OpenReadStream(@event.FilePath);
            await _tgClient.SendAudioAsync(
                @event.Aggregate.ChatId,
                readStream,
                @event.Title,
                cancellationToken);
        }
    }
}
