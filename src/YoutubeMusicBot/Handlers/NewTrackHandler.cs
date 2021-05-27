using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Handlers
{
	internal class NewTrackHandler :
		IRequestHandler<NewTrackHandler.Request, Unit>,
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

		public async Task<Unit> Handle(
			Request request,
			CancellationToken cancellationToken)
		{
			if (!request.File.Exists)
			{
				throw new ArgumentOutOfRangeException(
					nameof(request),
					request.File.Name,
					"File doesn't exists!");
			}

			var file = _file = request.File;

			if (request.TrySplit)
			{
				var split = await _mediator.Send(
					new TrySplitHandler.Request(
						request.File),
					cancellationToken);

				if (split)
					return Unit.Value;
			}

			// TODO: implement response
			if (file.Length > _botOptions.CurrentValue.MaxFileSize)
			{
				return Unit.Value;
			}

			await _tgClientWrapper.SendAudioAsync(file, cancellationToken);

			return Unit.Value;
		}

		public void Dispose()
		{
			_file?.Delete();
		}

		public record Request(
			FileInfo File,
			bool TrySplit = true) : IRequest
		{
		}
	}
}
