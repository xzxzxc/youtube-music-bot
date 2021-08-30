using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Music;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class MusicFileCreatedHandler : IEventHandler<MusicFileCreatedEvent, Message>
    {
        private readonly IMusicSplitter _musicSplitter;
        private readonly ITrackListParser _trackListParser;
        private readonly IFileSystem _fileSystem;
        private readonly IRepository<Message> _repository;
        private readonly IOptionsMonitor<BotOptions> _options;

        public MusicFileCreatedHandler(
            IMusicSplitter musicSplitter,
            ITrackListParser trackListParser,
            IFileSystem fileSystem,
            IRepository<Message> repository,
            IOptionsMonitor<BotOptions> options)
        {
            _musicSplitter = musicSplitter;
            _trackListParser = trackListParser;
            _fileSystem = fileSystem;
            _repository = repository;
            _options = options;
        }

        public async ValueTask Handle(
            MusicFileCreatedEvent @event,
            CancellationToken cancellationToken = default)
        {
            var aggregate = @event.Aggregate;
            if (@event.DescriptionFilePath != null)
            {
                var description = await _fileSystem.GetFileTextAsync(
                    @event.DescriptionFilePath,
                    cancellationToken);
                var tracks = _trackListParser.Parse(description).ToArray();
                if (tracks.Length > 1)
                {
                    await foreach (var filePath in _musicSplitter.SplitAsync(
                        @event.MusicFilePath,
                        tracks,
                        cancellationToken))
                    {
                        aggregate.MusicFileCreated(filePath, descriptionFilePath: null);
                        await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
                    }

                    return;
                }
            }

            var fileBytesCount = _fileSystem.GetFileBytesCount(@event.MusicFilePath);
            if (fileBytesCount < _options.CurrentValue.FileBytesLimit)
            {
                aggregate.FileToBeSentCreated(
                    @event.MusicFilePath,
                    _fileSystem.GetFileName(@event.MusicFilePath));
                await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
                return;
            }

            var splitBySilence = false;
            await foreach (var filePath in _musicSplitter.SplitBySilenceAsync(
                @event.MusicFilePath,
                cancellationToken))
            {
                splitBySilence = true;
                aggregate.MusicFileCreated(filePath, descriptionFilePath: null);
                await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
            }

            if (splitBySilence)
                return;

            var count = (int)Math.Round(
                fileBytesCount / (decimal)_options.CurrentValue.FileBytesLimit,
                MidpointRounding.ToPositiveInfinity);
            await foreach (var filePath in _musicSplitter.SplitInEqualPartsAsync(
                @event.MusicFilePath,
                count,
                cancellationToken))
            {
                aggregate.MusicFileCreated(filePath, descriptionFilePath: null);
                await _repository.SaveAndEmitEventsAsync(aggregate, cancellationToken);
            }
        }
    }
}
