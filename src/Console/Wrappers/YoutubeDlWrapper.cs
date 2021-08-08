using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Console.Handlers;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Wrappers.Interfaces;

namespace YoutubeMusicBot.Console.Wrappers
{
    public class YoutubeDlWrapper : IYoutubeDlWrapper
    {
        private static string? ConfigFilePath;
        private readonly MessageContext _messageContext;
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IProcessRunner _processRunner;
        private readonly IYoutubeDlConfigPath _youtubeDlConfigPath;
        private readonly IMediator _mediator;
        private readonly Regex _downloadingStarted;
        private readonly Regex _fileCompleted;
        private readonly string _cacheFolder;

        public YoutubeDlWrapper(
            MessageContext messageContext,
            ICacheFolder cacheFolder,
            ITgClientWrapper tgClientWrapper,
            IProcessRunner processRunner,
            IYoutubeDlConfigPath youtubeDlConfigPath,
            IMediator mediator)
        {
            _messageContext = messageContext;
            _tgClientWrapper = tgClientWrapper;
            _processRunner = processRunner;
            _youtubeDlConfigPath = youtubeDlConfigPath;
            _mediator = mediator;

            _cacheFolder = cacheFolder.Value;
            _fileCompleted = new Regex(
                @"^\[completed\] (?<file_name>.+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);
            _downloadingStarted = new Regex(
                @"^\[info\] Writing video description to: (\d|N)(\d|A)\d*-(?<title>.+)\.description$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public async Task DownloadAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            await foreach (var line in _processRunner.RunAsync(
                new ProcessRunner.Request(
                    "youtube-dl",
                    _cacheFolder,
                    Arguments: new[]
                    {
                        "--config-location",
                        await _youtubeDlConfigPath.GetValueAsync(cancellationToken),
                        url,
                    }),
                cancellationToken))
            {
                var startedMatch = _downloadingStarted.Match(line);
                if (startedMatch.Success)
                {
                    var title = _messageContext.Title = startedMatch.Groups["title"].Value;
                    var messageToUpdate = _messageContext.MessageToUpdate
                        ?? throw new InvalidOperationException(
                            $"{nameof(_messageContext.MessageToUpdate)} is not initialized!");
                    await _tgClientWrapper.UpdateMessageAsync(
                        messageToUpdate.Id,
                        $"Loading \"{title}\" started.",
                        messageToUpdate.InlineButton,
                        cancellationToken);
                    continue;
                }

                var completedMatch = _fileCompleted.Match(line);
                if (completedMatch.Success)
                {
                    var fileName = completedMatch.Groups["file_name"].Value;
                    var file = new FileInfoWrapper(
                            _cacheFolder,
                            fileName);
                    await _mediator.Send(
                        new NewTrackHandler.Request(file),
                        cancellationToken);
                    continue;
                }
            }
        }
    }
}
