using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Options;
using YoutubeMusicBot.Console.Wrappers.Interfaces;

namespace YoutubeMusicBot.Console.Handlers
{
    public class NewTrackHandler :
        IRequestHandler<NewTrackHandler.Request, Unit>,
        IDisposable
    {
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IOptionsMonitor<BotOptions> _botOptions;
        private readonly IMediator _mediator;
        private readonly IDescriptionService _descriptionService;

        private IFileInfo? _file;
        private IFileInfo? _descriptionFile;

        public NewTrackHandler(
            ITgClientWrapper tgClientWrapper,
            IOptionsMonitor<BotOptions> botOptions,
            IMediator mediator,
            IDescriptionService descriptionService)
        {
            _tgClientWrapper = tgClientWrapper;
            _botOptions = botOptions;
            _mediator = mediator;
            _descriptionService = descriptionService;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var file = _file = request.File;
            if (!file.Exists)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request),
                    file.Name,
                    "File doesn't exists!");
            }

            _descriptionFile = _descriptionService.GetFileOrNull(file);

            if (!request.SkipSplit)
            {
                var split = await _mediator.Send(
                    new TrySplitHandler.Request(
                        file,
                        ForceSplit: file.Length > _botOptions.CurrentValue.MaxFileBytesCount),
                    cancellationToken);

                if (split)
                    return Unit.Value;
            }

            await _tgClientWrapper.SendAudioAsync(file, cancellationToken);

            return Unit.Value;
        }

        public void Dispose()
        {
            if (_file?.Exists ?? false)
                _file.Delete();

            if (_descriptionFile?.Exists ?? false)
                _descriptionFile.Delete();
        }

        public record Request(IFileInfo File, bool SkipSplit = false) : IRequest;
    }
}
