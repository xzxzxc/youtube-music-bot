using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class YoutubeDlWrapper : IYoutubeDlWrapper
	{
		private readonly ILogger _logger;

		public YoutubeDlWrapper(
			ILogger<YoutubeDlWrapper> logger)
		{
			_logger = logger;
		}

		public async Task DownloadAsync(
			string folderPath,
			string url,
			CancellationToken cancellationToken = default)
		{
			var processInfo = new ProcessStartInfo
			{
				FileName = @"wsl.exe",
				ArgumentList =
				{
					"youtube-dl",
					url,
				},
				WorkingDirectory = folderPath,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

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
