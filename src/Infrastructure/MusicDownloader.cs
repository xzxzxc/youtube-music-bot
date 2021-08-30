using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Download;
using YoutubeMusicBot.Application.Models.Download;
using YoutubeMusicBot.Infrastructure.Abstractions;
using YoutubeMusicBot.Infrastructure.Models.ProcessRuner;

namespace YoutubeMusicBot.Infrastructure
{
    public class MusicDownloader : IMusicDownloader
    {
        private readonly IProcessRunner _processRunner;
        private readonly IFileSystem _fileSystem;
        private readonly string _youtubeDlConfigPath;
        private readonly Regex _fileCompleted;
        private readonly Regex _downloadingStarted;

        public MusicDownloader(
            IProcessRunner processRunner,
            IFileSystem fileSystem,
            IYoutubeDlConfigPath youtubeDlConfigPath)
        {
            _processRunner = processRunner;
            _fileSystem = fileSystem;
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
                new ProcessOptions(
                    "youtube-dl",
                    pathToDownloadTo,
                    Arguments: new[] { "--config-location", _youtubeDlConfigPath, url, }),
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
                    var filePath = _fileSystem.JoinPath(pathToDownloadTo, fileName);
                    var descriptionFileName = _fileSystem.ChangeExtension(fileName, "description");
                    var descriptionFilePath = _fileSystem.JoinPath(
                        pathToDownloadTo,
                        descriptionFileName);
                    if (!_fileSystem.IsFileExists(descriptionFilePath))
                        descriptionFilePath = null;
                    yield return new FileLoadedResult(filePath, descriptionFilePath);
                    continue;
                }
            }
        }
    }
}
