using System.Collections.Generic;
using System.Linq;
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
    internal class Mp3SplitWrapper : IMp3SplitWrapper
    {
        private readonly IProcessRunner _processRunner;
        private readonly IMediator _mediator;
        private readonly string _cacheFolder;
        private readonly Regex _regex;

        public Mp3SplitWrapper(
            ICacheFolder cacheFolder,
            IProcessRunner processRunner,
            IMediator mediator)
        {
            _processRunner = processRunner;
            _mediator = mediator;
            _cacheFolder = cacheFolder.Value;
            _regex = new Regex(
                @"^   File ""(?<file_name>.+)"" created$",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public async Task SplitAsync(
            IFileInfo file,
            IReadOnlyCollection<TrackModel> tracks,
            CancellationToken cancellationToken = default)
        {
            await foreach (var line in _processRunner.RunAsync(
                new ProcessRunner.Request(
                    "mp3splt",
                    _cacheFolder,
                    Arguments: new[]
                        {
                            "-q", "-g", $"%[@o,@b=#t,@t={tracks.First().Title}]"
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
                cancellationToken))
            {
                var match = _regex.Match(line);
                if (!match.Success)
                    continue;

                var newFile = new FileInfoWrapper(
                        _cacheFolder,
                        match.Groups["file_name"].Value);
                await _mediator.Send(
                    new NewTrackHandler.Request(newFile, TrySplit: false),
                    cancellationToken);
            }
        }
    }
}
