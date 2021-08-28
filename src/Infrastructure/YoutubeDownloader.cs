using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;
using YoutubeMusicBot.Application.Models.YoutubeDownloader;
using YoutubeMusicBot.Infrastructure.Wrappers;

namespace YoutubeMusicBot.Infrastructure
{
    public class YoutubeDownloader : IYoutubeDownloader
    {
        private readonly IProcessRunner _processRunner;
        private readonly string _youtubeDlConfigPath;
        private readonly Regex _fileCompleted;
        private readonly Regex _downloadingStarted;

        public YoutubeDownloader(
            IProcessRunner processRunner,
            IYoutubeDlConfigPath youtubeDlConfigPath)
        {
            _processRunner = processRunner;
            _youtubeDlConfigPath = youtubeDlConfigPath.Value;
            _fileCompleted = new Regex(
                @"^\[completed\] (?<file_name>.+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);
            _downloadingStarted = new Regex(
                @"^\[info\] Writing video description to: (\d|N)?(\d|A)\d*-(?<title>.+)\.description$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public async IAsyncEnumerable<IDownloadResult> DownloadAsync(
            string pathToDownloadTo,
            string url,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var line in _processRunner.RunAsync(
                new ProcessRunner.Request(
                    "youtube-dl",
                    pathToDownloadTo,
                    Arguments: new[]
                    {
                        "--config-location",
                        _youtubeDlConfigPath,
                        url,
                    }),
                cancellationToken))
            {
                var startedMatch = _downloadingStarted.Match(line);
                if (startedMatch.Success)
                {
                    var title = startedMatch.Groups["title"].Value;
                    yield return new RawTitleParsedResult(title);
                    continue;
                }

                var completedMatch = _fileCompleted.Match(line);
                if (completedMatch.Success)
                {
                    var fileName = completedMatch.Groups["file_name"].Value;
                    var file = new FileInfoWrapper(
                        pathToDownloadTo,
                        fileName);
                    yield return new FileLoadedResult(file);
                    continue;
                }
            }
        }
    }
}
