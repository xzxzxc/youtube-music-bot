using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Helpers;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models.YoutubeDownloader;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class LoadingProcessUserMessageSentHandler :
        IEventHandler<LoadingProcessMessageSentEvent, Message>
    {
        private readonly ITgClient _tgClient;
        private readonly IYoutubeDownloader _downloader;
        private readonly IFileSystem _fileSystem;

        public LoadingProcessUserMessageSentHandler(
            ITgClient tgClient,
            IYoutubeDownloader downloader,
            IFileSystem fileSystem)
        {
            _tgClient = tgClient;
            _downloader = downloader;
            _fileSystem = fileSystem;
        }

        public async ValueTask Handle(
            LoadingProcessMessageSentEvent @event,
            CancellationToken cancellationToken = default)
        {
            var tempFolderPath = _fileSystem.CreateTempFolder($"{@event.Id}");
            await foreach (var res in _downloader.DownloadAsync(
                tempFolderPath,
                @event.Aggregate.Text,
                cancellationToken))
            {
                switch (res)
                {
                    case FileLoadedResult fileLoaded:
                        @event.Aggregate.FileLoaded(fileLoaded.Value.FullName);
                        continue;
                    case RawTitleParsedResult titleParsed:
                        await _tgClient.UpdateMessageAsync(
                            @event.Aggregate.ChatId,
                            @event.MessageId,
                            $"Loading \"{titleParsed.Value}\" started.",
                            InlineButtonFactory.CreateCancel(@event),
                            cancellationToken);
                        continue;
                    default:
                        var resType = res.GetType();
                        throw new InvalidOperationException(
                            $"Unknown type of result: {resType.FullName ?? resType.Name}");
                }
            }
        }
    }
}
