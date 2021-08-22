using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Infrastructure.Wrappers
{
    public class Mp3SplitWrapper : IMp3SplitWrapper
    {
        private readonly IProcessRunner _processRunner;
        private readonly IOptionsMonitor<SplitOptions> _splitOptions;
        private readonly Regex _splitRegex;

        public Mp3SplitWrapper(
            IProcessRunner processRunner,
            IOptionsMonitor<SplitOptions> splitOptions)
        {
            _processRunner = processRunner;
            _splitOptions = splitOptions;
            _splitRegex = new Regex(
                @"^   File ""(?<file_name>.+)"" created$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public IAsyncEnumerable<IFileInfo> SplitAsync(
            IFileInfo file,
            TracksList tracks,
            CancellationToken cancellationToken = default)
        {
            var lines = _processRunner.RunAsync(
                new ProcessRunner.Request(
                    "mp3splt",
                    file.DirectoryName,
                    Arguments: new[]
                        {
                            "-q", "-g", $"%[@o,@b=#t,@t={tracks[0].Title}]"
                            + string.Concat(
                                tracks
                                    .Skip(1)
                                    .Select(t => $"[@t={t.Title}]")),
                            file.Name,
                        }
                        .Concat(
                            tracks.Select(
                                track =>
                                    $"{track.Start.Minutes}.{track.Start.Seconds}"))
                        // mean that we want split including last track
                        .Append("EOF")
                        .ToArray()),
                cancellationToken);
            return GetFiles(lines, file, cancellationToken);
        }

        public IAsyncEnumerable<IFileInfo> SplitBySilenceAsync(
            IFileInfo file,
            CancellationToken cancellationToken = default)
        {
            var lines = _processRunner.RunAsync(
                new ProcessRunner.Request(
                    ProcessName: "mp3splt",
                    WorkingDirectory: file.DirectoryName,
                    "-s",
                    "-q",
                    "-p",
                    $"min={_splitOptions.CurrentValue.MinSilenceLength.TotalSeconds}",
                    file.Name),
                cancellationToken);

            return GetFiles(lines, file, cancellationToken);
        }

        public IAsyncEnumerable<IFileInfo> SplitInEqualPartsAsync(
            IFileInfo file,
            int partsCount,
            CancellationToken cancellationToken = default)
        {
            var lines = _processRunner.RunAsync(
                new ProcessRunner.Request(
                    ProcessName: "mp3splt",
                    WorkingDirectory: file.DirectoryName,
                    "-q",
                    "-S",
                    partsCount.ToString(),
                    file.Name),
                cancellationToken);

            return GetFiles(lines, file, cancellationToken);
        }

        private async IAsyncEnumerable<IFileInfo> GetFiles(
            IAsyncEnumerable<ProcessRunner.Line> lines,
            IFileInfo file,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var line in lines.WithCancellation(cancellationToken))
            {
                var match = _splitRegex.Match(line);
                if (!match.Success)
                    continue;

                yield return new FileInfoWrapper(
                    file.DirectoryName,
                    match.Groups["file_name"].Value);
            }
        }
    }
}
