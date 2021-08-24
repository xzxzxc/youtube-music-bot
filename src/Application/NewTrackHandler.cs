using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Application
{
    public class NewTrackHandler :
        IRequestHandler<NewTrackHandler.Request, bool>,
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

        public async ValueTask<bool> Handle(
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

            var fileIsTooLarge = file.Length > _botOptions.CurrentValue.MaxFileBytesCount;
            if (!request.SkipSplit)
            {
                var split = await _mediator.Send<TrySplitHandler.Request, bool>(
                    new TrySplitHandler.Request(
                        file,
                        fileIsTooLarge),
                    cancellationToken);

                if (split) // if force split - this would be true in any way
                    return true;
            }

            if (fileIsTooLarge)
                return false;

            await _tgClientWrapper.SendAudioAsync(file, cancellationToken);

            return true;
        }

        public void Dispose()
        {
            if (_file?.Exists ?? false)
                _file.Delete();

            if (_descriptionFile?.Exists ?? false)
                _descriptionFile.Delete();
        }

        public record Request(IFileInfo File, bool SkipSplit = false)
            : IRequest<bool>;
    }
}
