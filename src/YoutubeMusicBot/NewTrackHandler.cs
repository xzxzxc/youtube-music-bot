using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YoutubeMusicBot
{
	internal class NewTrackHandler :
		INotificationHandler<NewTrackHandler.Notification>
	{
		private readonly ITelegramBotClient _telegramBotClient;

		public NewTrackHandler(ITelegramBotClient telegramBotClient)
		{
			_telegramBotClient = telegramBotClient;
		}

		public async Task Handle(
			Notification notification,
			CancellationToken cancellationToken)
		{
			var file = notification.File;

			await using var fileStream = file.OpenRead();

			// TODO: add retry policy
			try
			{
				await _telegramBotClient.SendAudioAsync(
					notification.ChatId,
					new InputMedia(
						fileStream,
						file.Name
						?? throw new InvalidOperationException())); // TODO:
			}
			finally
			{
				file.Delete();
			}
		}

		public record Notification(
			long ChatId,
			FileInfo File) : INotification
		{
		}
	}
}
