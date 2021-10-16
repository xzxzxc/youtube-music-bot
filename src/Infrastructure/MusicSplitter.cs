using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Music;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Models.Music;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Infrastructure.Abstractions;
using YoutubeMusicBot.Infrastructure.Models.ProcessRuner;
using YoutubeMusicBot.Infrastructure.Options;

namespace YoutubeMusicBot.Infrastructure
{
    public class MusicSplitter : IMusicSplitter
    {
        private readonly IFileSystem _fileSystem;
        private readonly IProcessRunner _processRunner;
        private readonly IOptionsMonitor<SplitOptions> _splitOptions;
        private readonly Regex _splitRegex;

        public MusicSplitter(
            IFileSystem fileSystem,
            IProcessRunner processRunner,
            IOptionsMonitor<SplitOptions> splitOptions)
        {
            _fileSystem = fileSystem;
            _processRunner = processRunner;
            _splitOptions = splitOptions;
            _splitRegex = new Regex(
                @"^   File ""(?<file_name>.+)"" created$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public IAsyncEnumerable<string> SplitAsync(
            string filePath,
            IReadOnlyList<Track> tracks,
            CancellationToken cancellationToken = default)
        {
            var workingDirectory = _fileSystem.GetFileDirectoryPath(filePath);
            var fileName = _fileSystem.GetFileName(filePath);
            var lines = _processRunner.RunAsync(
                new ProcessOptions(
                    "mp3splt",
                    workingDirectory,
                    Arguments: new[]
                        {
                            "-q", "-g", $"%[@o,@b=#t,@t={tracks[0].Title}]"
                            + string.Concat(
                                tracks
                                    .Skip(1)
                                    .Select(t => $"[@t={t.Title}]")),
                            fileName,
                        }
                        .Concat(
                            tracks.Select(
                                track =>
                                    $"{(int)track.Start.TotalMinutes}.{track.Start.Seconds}"))
                        // mean that we want split including last track
                        .Append("EOF")
                        .ToArray()),
                cancellationToken);
            return GetFiles(lines, workingDirectory, cancellationToken);
        }

        public IAsyncEnumerable<string> SplitBySilenceAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var workingDirectory = _fileSystem.GetFileDirectoryPath(filePath);
            var fileName = _fileSystem.GetFileName(filePath);
            var lines = _processRunner.RunAsync(
                new ProcessOptions(
                    ProcessName: "mp3splt",
                    WorkingDirectory: workingDirectory,
                    "-s",
                    "-q",
                    "-p",
                    $"min={_splitOptions.CurrentValue.MinSilenceLength.TotalSeconds}",
                    fileName),
                cancellationToken);

            return GetFiles(lines, workingDirectory, cancellationToken);
        }

        public IAsyncEnumerable<string> SplitInEqualPartsAsync(
            string filePath,
            int partsCount,
            CancellationToken cancellationToken = default)
        {
            var workingDirectory = _fileSystem.GetFileDirectoryPath(filePath);
            var fileName = _fileSystem.GetFileName(filePath);
            var lines = _processRunner.RunAsync(
                new ProcessOptions(
                    ProcessName: "mp3splt",
                    WorkingDirectory: workingDirectory,
                    "-q",
                    "-S",
                    partsCount.ToString(),
                    fileName),
                cancellationToken);

            return GetFiles(lines, workingDirectory, cancellationToken);
        }

        private async IAsyncEnumerable<string> GetFiles(
            IAsyncEnumerable<ProcessResultLine> lines,
            string workingDirectory,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var line in lines.WithCancellation(cancellationToken))
            {
                var match = _splitRegex.Match(line);
                if (!match.Success)
                    continue;

                yield return _fileSystem.JoinPath(
                    workingDirectory,
                    match.Groups["file_name"].Value);
            }
        }
    }
}
