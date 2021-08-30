using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Download;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.Models.Download;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.EventHandlers
{
    public class LoadingProcessUserMessageSentHandler :
        IEventHandler<LoadingProcessMessageSentEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly IMusicDownloader _downloader;
        private readonly IFileSystem _fileSystem;
        private readonly ICallbackDataFactory _callbackDataFactory;
        private readonly IRepository<Message> _repository;

        public LoadingProcessUserMessageSentHandler(
            ITgClient tgClient,
            IMusicDownloader downloader,
            IFileSystem fileSystem,
            ICallbackDataFactory callbackDataFactory,
            IRepository<Message> repository)
        {
            _tgClient = tgClient;
            _downloader = downloader;
            _fileSystem = fileSystem;
            _callbackDataFactory = callbackDataFactory;
            _repository = repository;
        }

        public async ValueTask Handle(
            LoadingProcessMessageSentEvent @event,
            CancellationToken cancellationToken = default)
        {
            var tempFolderPath = _fileSystem.GetOrCreateTempFolder(@event.AggregateId);
            await foreach (var res in _downloader.DownloadAsync(
                tempFolderPath,
                @event.Aggregate.Text,
                cancellationToken))
            {
                switch (res)
                {
                    case FileLoadedResult fileLoaded:
                        @event.Aggregate.MusicFileCreated(
                            fileLoaded.MusicFilePath,
                            fileLoaded.DescriptionFilePath);
                        await _repository.SaveAndEmitEventsAsync(
                            @event.Aggregate,
                            cancellationToken);
                        continue;
                    case RawTitleParsedResult titleParsed:
                        await _tgClient.UpdateMessageAsync(
                            @event.Aggregate.ChatId,
                            @event.MessageId,
                            $"Loading \"{titleParsed.Value}\" started.",
                            new InlineButton(
                                "Cancel",
                                _callbackDataFactory.CreateForCancel(@event))
                                .ToCollection(),
                            cancellationToken);
                        continue;
                    default:
                        var resType = res.GetType();
                        throw new InvalidOperationException(
                            $"Unknown type of result: {resType.FullName ?? resType.Name}");
                }
            }

            @event.Aggregate.Finished();
            await _repository.SaveAndEmitEventsAsync(@event.Aggregate, cancellationToken);
        }
    }
}
