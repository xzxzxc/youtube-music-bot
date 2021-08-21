using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Infrastructure.Wrappers
{
    public class YoutubeDlWrapper : IYoutubeDlWrapper
    {
        private readonly MessageContext _messageContext;
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IProcessRunner _processRunner;
        private readonly IYoutubeDlConfigPath _youtubeDlConfigPath;
        private readonly Regex _downloadingStarted;
        private readonly Regex _fileCompleted;
        private readonly string _cacheFolder;

        public YoutubeDlWrapper(
            MessageContext messageContext,
            ICacheFolder cacheFolder,
            ITgClientWrapper tgClientWrapper,
            IProcessRunner processRunner,
            IYoutubeDlConfigPath youtubeDlConfigPath)
        {
            _messageContext = messageContext;
            _tgClientWrapper = tgClientWrapper;
            _processRunner = processRunner;
            _youtubeDlConfigPath = youtubeDlConfigPath;

            _cacheFolder = cacheFolder.Value;
            _fileCompleted = new Regex(
                @"^\[completed\] (?<file_name>.+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);
            _downloadingStarted = new Regex(
                @"^\[info\] Writing video description to: (\d|N)(\d|A)\d*-(?<title>.+)\.description$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public async IAsyncEnumerable<IFileInfo> DownloadAsync(
            string url,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    await _tgClientWrapper.UpdateMessageAsync(
                        $"Loading \"{title}\" started.",
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
                    yield return file;
                }
            }
        }
    }
}
