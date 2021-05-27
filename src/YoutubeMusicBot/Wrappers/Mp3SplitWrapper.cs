using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	internal class Mp3SplitWrapper : IMp3SplitWrapper
	{
		private readonly string _cacheFolder;
		private readonly IMediator _mediator;
		private readonly Regex _regex;

		public Mp3SplitWrapper(
			ICacheFolder cacheFolder,
			IMediator mediator)
		{
			_cacheFolder = cacheFolder.Value;
			_mediator = mediator;
			_regex = new Regex(
				@"^   File ""(?<file_name>.+)"" created$",
				RegexOptions.Compiled | RegexOptions.Multiline);
		}

		public async Task SplitAsync(
			FileInfo file,
			IReadOnlyCollection<TrackModel> tracks,
			CancellationToken cancellationToken = default)
		{
			await _mediator.Send(
				new RunProcessHandler.Request(
					"mp3splt",
					_cacheFolder,
					ProcessOutput,
					Arguments: new[]
						{
							"-q",
							"-g",
							$"%[@o,@b=#t,@t={tracks.First().Title}]"
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
		}

		private async Task ProcessOutput(string line, CancellationToken cancellationToken)
		{
			var match = _regex.Match(line);
			if (match.Success)
			{
				var file = new FileInfo(
					Path.Join(
						_cacheFolder,
						match.Groups["file_name"].Value));
				await _mediator.Send(
					new NewTrackHandler.Request(file, TrySplit: false),
                    cancellationToken);
			}
		}
	}
}
