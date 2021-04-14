using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class Mp3SplitWrapper : IMp3SplitWrapper
	{
		private readonly ILogger _logger;

		public Mp3SplitWrapper(
			ILogger<Mp3SplitWrapper> logger)
		{
			_logger = logger;
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
					"-g",
					$"%[@o,@b=#t,@t=\"{tracks.First().Name}\"]"
					+
					string.Concat(
						tracks
							.Skip(1)
							.Select(t => $"[@t=\"{t.Name}\"]")),
					file.Name,
					//string.Concat(
					//	tracks
					//		.Select(
					//			t => $"{t.Start.Minutes}.{t.Start.Seconds}")),
					//"EOF"
				},
				WorkingDirectory = file.DirectoryName
					?? throw new InvalidOperationException(), // TODO:
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
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

			// TODO: think about what to do
			process.OutputDataReceived += (
					object sender,
					DataReceivedEventArgs e) =>
				Console.WriteLine("output>>" + e.Data);

			process.ErrorDataReceived += (
				object sender,
				DataReceivedEventArgs e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
					_logger.LogError(e.Data);
			};

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			await process.WaitForExitAsync(cancellationToken);
			process.Close();
		}
	}
}
