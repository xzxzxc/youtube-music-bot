using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using MediatR;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	internal class MessageHandler : INotificationHandler<MessageHandler.Message>
	{
		private readonly IIndex<ChatContext, ITrackFilesWatcher> _trackFilesWatchers;
		private readonly IYoutubeDlWrapper _youtubeDlWrapper;

		public MessageHandler(
			IIndex<ChatContext, ITrackFilesWatcher> trackFilesWatchers,
			IYoutubeDlWrapper youtubeDlWrapper)
		{
			_trackFilesWatchers = trackFilesWatchers;
			_youtubeDlWrapper = youtubeDlWrapper;
		}

		public async Task Handle(
			Message notification,
			CancellationToken cancellationToken = default)
		{
			var message = notification.Value
				?? throw new ArgumentNullException(nameof(notification));

			var trackFilesWatcher = _trackFilesWatchers[message.Chat.ToContext()];

			await _youtubeDlWrapper.DownloadAsync(
					trackFilesWatcher.ChatFolderPath,
					message.Text,
					cancellationToken);
		}

		public record Message(Telegram.Bot.Types.Message? Value) : INotification
		{
		}
	}
}
