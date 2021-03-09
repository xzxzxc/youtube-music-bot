using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace YoutubeMusicBot
{
	class YoutubeDlWrapper : IYoutubeDlWrapper
	{
		private readonly ILogger _logger;
		private readonly IOptionsMonitor<DownloadOptions> _downloadOptions;

		public YoutubeDlWrapper(
			ILogger<YoutubeDlWrapper> logger,
			IOptionsMonitor<DownloadOptions> downloadOptions)
		{
			_logger = logger;
			_downloadOptions = downloadOptions;
		}

		public async Task<IFileWrapper> DownloadAsync(
			string url,
			CancellationToken cancellationToken = default)
		{
			var directoryInfo = new DirectoryInfo(
				Path.Join(
					_downloadOptions.CurrentValue.CacheFilesFolderPath,
					$"{Guid.NewGuid()}"));
			directoryInfo.Create();

			var processInfo = new ProcessStartInfo()
			{
				FileName = @"wsl.exe",
				ArgumentList =
				{
					"youtube-dl",
					url,
				},
				WorkingDirectory = directoryInfo.FullName,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			using var process = new Process
			{
				StartInfo = processInfo,
			};

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

			return new FileWrapper(
				directoryInfo.EnumerateFiles("*.mp3").First(),
				async () => directoryInfo.Delete(recursive: true));
		}
	}
}