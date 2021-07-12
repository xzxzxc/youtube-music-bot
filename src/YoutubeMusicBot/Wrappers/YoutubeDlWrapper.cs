using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
    public class YoutubeDlWrapper : IYoutubeDlWrapper
    {
        private static string? ConfigFilePath;
        private readonly MessageContext _messageContext;
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IProcessRunner _processRunner;
        private readonly ILinuxPathResolver _linuxPathResolver;
        private readonly IMediator _mediator;
        private readonly Regex _downloadingStarted;
        private readonly Regex _fileCompleted;
        private readonly string _cacheFolder;

        public YoutubeDlWrapper(
            MessageContext messageContext,
            ICacheFolder cacheFolder,
            ITgClientWrapper tgClientWrapper,
            IProcessRunner processRunner,
            ILinuxPathResolver linuxPathResolver,
            IMediator mediator)
        {
            _messageContext = messageContext;
            _tgClientWrapper = tgClientWrapper;
            _processRunner = processRunner;
            _linuxPathResolver = linuxPathResolver;
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
            ConfigFilePath ??= await _linuxPathResolver.Resolve(
                    Path.Join(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "youtube-dl.conf"),
                cancellationToken);
            await foreach (var line in _processRunner.RunAsync(
                new ProcessRunner.Request(
                    "youtube-dl",
                    _cacheFolder,
                    Arguments: new[]
                    {
                        "--config-location",
                        ConfigFilePath,
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
                    var file = new FileInfo(
                        Path.Join(
                            _cacheFolder,
                            fileName));
                    await _mediator.Send(
                        new NewTrackHandler.Request(file),
                        cancellationToken);
                    continue;
                }
            }
        }
    }
}
