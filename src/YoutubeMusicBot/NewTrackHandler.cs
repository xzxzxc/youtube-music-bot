using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	internal class NewTrackHandler :
		INotificationHandler<NewTrackHandler.Notification>,
		IDisposable
	{
		private readonly ITgClientWrapper _tgClientWrapper;
		private readonly IOptionsMonitor<BotOptions> _botOptions;
		private readonly IMediator _mediator;
		private FileInfo? _file;

		public NewTrackHandler(
			ITgClientWrapper tgClientWrapper,
			IOptionsMonitor<BotOptions> botOptions,
			IMediator mediator)
		{
			_tgClientWrapper = tgClientWrapper;
			_botOptions = botOptions;
			_mediator = mediator;
		}

		public async Task Handle(
			Notification notification,
			CancellationToken cancellationToken)
		{
			var file = _file = notification.File;

			var split = await _mediator.Send(
				new TrySplitHandler.Notification(
					notification.Chat,
					notification.File),
				cancellationToken);

			if (split)
				return;

			// TODO: implement response
			if (file.Length > _botOptions.CurrentValue.MaxFileSize)
			{
			}

			// TODO: add retry policy
			await _tgClientWrapper.SendAudioAsync(notification.Chat, file);
		}

		public void Dispose()
		{
			// TODO: check this called in case of exception
			_file?.Delete();
		}

		public record Notification(
			ChatContext Chat,
			FileInfo File) : INotification
		{
		}
	}
}
