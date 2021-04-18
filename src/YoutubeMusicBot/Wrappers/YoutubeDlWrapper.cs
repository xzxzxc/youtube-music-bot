using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class YoutubeDlWrapper : IYoutubeDlWrapper
	{
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		private readonly Regex _fileCompleted;
		private readonly string _cacheFolder;

		public YoutubeDlWrapper(
			ICacheFolder cacheFolder,
			ILogger<YoutubeDlWrapper> logger,
			IMediator mediator)
		{
			_logger = logger;
			_mediator = mediator;

			_cacheFolder = cacheFolder.Value;
			_fileCompleted = new Regex(
				@"^file for bot: '(?<file_name>.+)'$",
				RegexOptions.Compiled | RegexOptions.Multiline);
		}

		public async Task DownloadAsync(
			string url,
			CancellationToken cancellationToken = default)
		{
			var processInfo = new ProcessStartInfo
			{
				FileName = @"wsl.exe",
				ArgumentList =
				{
					"-e",
					"youtube-dl",
					url,
				},
				WorkingDirectory = _cacheFolder,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				StandardErrorEncoding = Encoding.UTF8,
				StandardOutputEncoding = Encoding.UTF8,
			};

			using var process = new Process
			{
				StartInfo = processInfo,
			};

			process.Start();

			Task<string?>? readOutputTask = null;
			Task<string?>? readErrorTask = null;
			while ((readOutputTask == null && !process.StandardOutput.EndOfStream)
				|| (readErrorTask == null && !process.StandardError.EndOfStream))
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

			var match = _fileCompleted.Match(line);
			if (match.Success)
			{
				var file = new FileInfo(
					Path.Join(
						_cacheFolder,
						match.Groups["file_name"].Value));
				await _mediator.Send(
					new NewTrackHandler.Request(file));
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
