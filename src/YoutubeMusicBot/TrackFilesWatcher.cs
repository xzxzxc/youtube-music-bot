using System;
using System.IO;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;

namespace YoutubeMusicBot
{
	internal class TrackFilesWatcher : ITrackFilesWatcher, IDisposable
	{
		private readonly IMediator _mediator;
		private readonly FileSystemWatcher _watcher;
		private readonly ChatContext _chat;

		public TrackFilesWatcher(
			ChatContext chat,
			IOptionsMonitor<DownloadOptions> downloadOptions,
			IMediator mediator)
		{
			_chat = chat;
			_mediator = mediator;

			var chatFolderPath = ChatFolderPath = Path.Join(
				downloadOptions.CurrentValue.CacheFilesFolderPath,
				$"{chat.Id}");

			Directory.CreateDirectory(chatFolderPath);

			_watcher = new FileSystemWatcher(chatFolderPath)
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = false,
				Filter = "*.mp3",
				NotifyFilter = NotifyFilters.FileName
					| NotifyFilters.Size,
			};

			_watcher.Created += CreatedOrRenamed;
			_watcher.Renamed += CreatedOrRenamed;
		}

		public string ChatFolderPath { get; }

		private void CreatedOrRenamed(object _, FileSystemEventArgs args)
		{
			var fileInfo = new FileInfo(args.FullPath);
			if (!fileInfo.Exists
				|| fileInfo.Name.EndsWith(".temp.mp3")
				|| !fileInfo.Name.EndsWith(".mp3")
				|| fileInfo.Length == 0)
			{
				return;
			}

			_mediator.Publish(
				new NewTrackHandler.Notification(
					_chat,
					fileInfo));
		}

		public void Dispose()
		{
			_watcher.Dispose();
		}
	}
}
