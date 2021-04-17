using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class Mp3SplitWrapper : IMp3SplitWrapper
	{
		private readonly string _cacheFolder;
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		private readonly Regex _regex;

		public Mp3SplitWrapper(
			ICacheFolder cacheFolder,
			IMediator mediator,
			ILogger<Mp3SplitWrapper> logger)
		{
			_cacheFolder = cacheFolder.Value;
			_logger = logger;
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
			var processInfo = new ProcessStartInfo
			{
				FileName = @"wsl.exe",
				ArgumentList =
				{
					"mp3splt",
					"-q",
					"-g",
					$"%[@o,@b=#t,@t={tracks.First().Title}]"
					+ string.Concat(
						tracks
							.Skip(1)
							.Select(t => $"[@t={t.Title}]")),
					file.Name,
				},
				WorkingDirectory = _cacheFolder,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				StandardErrorEncoding = Encoding.UTF8,
				StandardOutputEncoding = Encoding.UTF8,
			};

			foreach (var track in tracks)
			{
				processInfo.ArgumentList.Add(
					$"{track.Start.Minutes}.{track.Start.Seconds}");
			}

			// mean that we want split including last track
			processInfo.ArgumentList.Add("EOF");

			using var process = new Process
			{
				StartInfo = processInfo,
			};

			process.Start();

			Task<string?>? readOutputTask = null;
			Task<string?>? readErrorTask = null;
			while ((readOutputTask == null
					&& !process.StandardOutput.EndOfStream)
				|| (readErrorTask == null
					&& !process.StandardError.EndOfStream))
			{
				readOutputTask ??= process.StandardOutput.ReadLineAsync();
				readErrorTask ??= process.StandardError.ReadLineAsync();

				var resTask = await Task.WhenAny(
					readOutputTask,
					readErrorTask);

				if (resTask == readOutputTask)
				{
					await ProcessOutput(readOutputTask.Result);
					readOutputTask = null;
				}
				else if (resTask == readErrorTask)
				{
					await ProcessError(readErrorTask.Result);
					readErrorTask = null;
				}
			}

			await process.WaitForExitAsync(cancellationToken);
			process.Close();
		}

		private async Task ProcessOutput(string? line)
		{
			if (string.IsNullOrEmpty(line))
				return;

			var match = _regex.Match(line);
			if (match.Success)
			{
				var file = new FileInfo(
					Path.Join(
						_cacheFolder,
						match.Groups["file_name"].Value));
				await _mediator.Send(
					new NewTrackHandler.Request(file, TrySplit: false));
			}

			_logger.LogInformation(line);
		}

		private async Task ProcessError(string? line)
		{
			if (string.IsNullOrEmpty(line))
				return;

			_logger.LogError(line);
		}
	}
}
