using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;

namespace YoutubeMusicBot
{
	internal class MessageHandler : INotificationHandler<MessageHandler.Message>
	{
		private readonly ITrackFilesWatcher _trackFilesWatcher;
		private readonly IYoutubeDlWrapper _youtubeDlWrapper;

		public MessageHandler(
			ITrackFilesWatcher trackFilesWatcher,
			IYoutubeDlWrapper youtubeDlWrapper)
		{
			_trackFilesWatcher = trackFilesWatcher;
			_youtubeDlWrapper = youtubeDlWrapper;
		}

		public async Task Handle(
			Message notification,
			CancellationToken cancellationToken = default)
		{
			var message = notification.Value
				?? throw new ArgumentNullException(nameof(notification));

			var chatFolderPath = _trackFilesWatcher.StartWatch(
				message.Chat.ToContext());

			await _youtubeDlWrapper.DownloadAsync(
					chatFolderPath,
					message.Text,
					cancellationToken);
		}

		public record Message(Telegram.Bot.Types.Message? Value) : INotification
		{
		}
	}
}
