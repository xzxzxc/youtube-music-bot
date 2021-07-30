using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Handlers
{
    public class TrySplitHandler :
        IRequestHandler<TrySplitHandler.Request, bool>
    {
        private readonly IMp3SplitWrapper _mp3SplitWrapper;
        private readonly ITrackListParser _trackListParser;
        private readonly IDescriptionService _descriptionService;

        public TrySplitHandler(
            IMp3SplitWrapper mp3SplitWrapper,
            ITrackListParser trackListParser,
            IDescriptionService descriptionService)
        {
            _mp3SplitWrapper = mp3SplitWrapper;
            _trackListParser = trackListParser;
            _descriptionService = descriptionService;
        }

        public async Task<bool> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var file = request.File;
            var descriptionFile = _descriptionService.GetFileOrNull(file);

            if (descriptionFile == null)
                return false;

            var description = await descriptionFile.GetTextAsync(cancellationToken);

            if (string.IsNullOrEmpty(description))
                return false;

            var trackList = _trackListParser.Parse(description)
                .ToArray();
            if (trackList.Length < 2)
                return false;

            await _mp3SplitWrapper.SplitAsync(
                request.File,
                trackList,
                cancellationToken);

            return true;
        }

        public record Request(IFileInfo File) : IRequest<bool>;
    }
}
