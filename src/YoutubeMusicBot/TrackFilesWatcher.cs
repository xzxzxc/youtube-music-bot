using System;
using System.Collections.Concurrent;
using System.IO;
using MediatR;
using Microsoft.Extensions.Options;

namespace YoutubeMusicBot
{
	internal class TrackFilesWatcher : ITrackFilesWatcher, IDisposable
	{
		private readonly IOptionsMonitor<DownloadOptions> _downloadOptions;
		private readonly IMediator _mediator;

		private readonly ConcurrentDictionary<long, FileSystemWatcher> _cache =
			new();

		public TrackFilesWatcher(
			IOptionsMonitor<DownloadOptions> downloadOptions,
			IMediator mediator)
		{
			_downloadOptions = downloadOptions;
			_mediator = mediator;
		}

		public string StartWatch(long chatId)
		{
			var cacheFolderPath = Path.Join(
				_downloadOptions.CurrentValue.CacheFilesFolderPath,
				$"{chatId}");
			Directory.CreateDirectory(cacheFolderPath);

			var newWatcher = new FileSystemWatcher(cacheFolderPath)
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = false,
				Filter = "*.mp3",
				NotifyFilter = NotifyFilters.FileName
					| NotifyFilters.Size,
			};

			if (!_cache.TryAdd(chatId, newWatcher))
				return cacheFolderPath;

			newWatcher.Renamed += (_, args) =>
			{
				var fileInfo = new FileInfo(args.FullPath);
				if (!fileInfo.Exists
					|| fileInfo.Name.EndsWith(".temp.mp3")
					|| !fileInfo.Name.EndsWith(".mp3"))
				{
					return;
				}

				_mediator.Publish(
					new NewTrackHandler.Notification(
						chatId,
						fileInfo));
			};

			return cacheFolderPath;
		}

		public void Dispose()
		{
			//TODO: dispose cache
		}
	}
}
