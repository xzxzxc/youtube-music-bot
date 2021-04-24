using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class YoutubeDlWrapper : IYoutubeDlWrapper
	{
		private readonly ITgClientWrapper _tgClientWrapper;
		private readonly IMediator _mediator;
		private readonly Regex _downloadingStarted;
		private readonly Regex _fileCompleted;
		private readonly string _cacheFolder;

		public YoutubeDlWrapper(
			ICacheFolder cacheFolder,
			ITgClientWrapper tgClientWrapper,
			IMediator mediator)
		{
			_tgClientWrapper = tgClientWrapper;
			_mediator = mediator;

			_cacheFolder = cacheFolder.Value;
			_fileCompleted = new Regex(
				@"^\[completed\] (?<file_name>.+)$",
				RegexOptions.Compiled | RegexOptions.Multiline);
			_downloadingStarted = new Regex(
				@"^\[info\] Writing video description to: (\d|N)(\d|A)\d*-(?<title>.+)\.description$",
				RegexOptions.Compiled | RegexOptions.Multiline);
		}

		public Task DownloadAsync(
			string url,
			CancellationToken cancellationToken = default) =>
			_mediator.Send(
				new RunProcessHandler.Request(
					"youtube-dl",
					_cacheFolder,
					ProcessOutput,
					Arguments: url),
				cancellationToken);

		private async Task ProcessOutput(string line)
		{
			var startedMatch = _downloadingStarted.Match(line);
			if (startedMatch.Success)
			{
				var title = startedMatch.Groups["title"].Value;
				await _tgClientWrapper.SendMessageAsync(
					$"Loading \"{title}\" started.");
				return;
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
					new NewTrackHandler.Request(file));
				return;
			}
		}
	}
}
