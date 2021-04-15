using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;

namespace YoutubeMusicBot
{
	internal class TrackFilesWatcher : ITrackFilesWatcher, IDisposable
	{
		private readonly IMediator _mediator;
		private readonly ILogger<TrackFilesWatcher> _logger;
		private readonly FileSystemWatcher _watcher;
		private readonly ChatContext _chat;
		private readonly IOptionsMonitor<DownloadOptions> _downloadOptions;
		private readonly ConcurrentDictionary<string, FileInfo> _files = new();
		private readonly CancellationTokenSource _loopStoppingToken;

		public TrackFilesWatcher(
			ChatContext chat,
			IOptionsMonitor<DownloadOptions> downloadOptions,
			IMediator mediator,
			ILogger<TrackFilesWatcher> logger)
		{
			_chat = chat;
			_downloadOptions = downloadOptions;
			_mediator = mediator;
			_logger = logger;

			var chatFolderPath = ChatFolderPath = Path.Join(
				downloadOptions.CurrentValue.CacheFilesFolderPath,
				$"{chat.Id}");

			Directory.CreateDirectory(chatFolderPath);

			_watcher = new FileSystemWatcher(chatFolderPath)
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = false,
				Filter = "*.mp3",
				NotifyFilter = NotifyFilters.FileName,
			};

			_watcher.Created += Created;
			_loopStoppingToken = new CancellationTokenSource();
			FileLockCheckLoop(_loopStoppingToken.Token);
		}

		public string ChatFolderPath { get; }

		private void Created(object _, FileSystemEventArgs args)
		{
			var filePath = args.FullPath;
			if (!filePath.EndsWith(".temp.mp3"))
				_files.TryAdd(filePath, new FileInfo(filePath));
		}

		private async Task FileLockCheckLoop(CancellationToken token)
		{
			while (true)
			{
				try
				{
					await Task.Delay(
						_downloadOptions.CurrentValue.TrackWatchDelay,
						token);

					foreach (var (key, file) in _files)
					{
						if (IsFileLocked(file))
							continue;

						var notification = new NewTrackHandler.Notification(
							_chat,
							file);
						_mediator.Publish(notification, token);
						_files.TryRemove(key, out _);
					}
				}
				catch (TaskCanceledException)
				{
				}
				catch (Exception ex)
				{
					_logger.LogCritical(
						ex,
						"Unexpected exception during file lock loop.",
						_chat);
				}
			}
		}

		private bool IsFileLocked(FileInfo file)
		{
			try
			{
				using var stream = file.Open(
					FileMode.Open,
					FileAccess.Read,
					FileShare.None);
				stream.Close();
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}

			//file is not locked
			return false;
		}

		public void Dispose()
		{
			_watcher.Dispose();
			_loopStoppingToken.Cancel();
		}

		private record DelayedTask(CancellationTokenSource Source, Task Task);
	}
}
