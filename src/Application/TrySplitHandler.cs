using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Application
{
    public class TrySplitHandler :
        IRequestHandler<TrySplitHandler.Request, bool>
    {
        private readonly IMp3SplitWrapper _mp3SplitWrapper;
        private readonly ITrackListParser _trackListParser;
        private readonly IDescriptionService _descriptionService;
        private readonly IMediator _mediator;
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IOptionsMonitor<BotOptions> _botOptions;

        public TrySplitHandler(
            IMp3SplitWrapper mp3SplitWrapper,
            ITrackListParser trackListParser,
            IDescriptionService descriptionService,
            IMediator mediator,
            ITgClientWrapper tgClientWrapper,
            IOptionsMonitor<BotOptions> botOptions)
        {
            _mp3SplitWrapper = mp3SplitWrapper;
            _trackListParser = trackListParser;
            _descriptionService = descriptionService;
            _mediator = mediator;
            _tgClientWrapper = tgClientWrapper;
            _botOptions = botOptions;
        }

        public async ValueTask<bool> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var file = request.File;

            var trackList = await GetTrackListFromDescriptionOrNull(file, cancellationToken);

            IAsyncEnumerable<IFileInfo> files;
            if (trackList != null)
            {
                files = _mp3SplitWrapper.SplitAsync(
                    request.File,
                    trackList,
                    cancellationToken);

                if (!await SendNewTrackRequests(files, cancellationToken))
                    throw new InvalidOperationException("Track list was parsed, but wasn't split.");

                return true;
            }

            if (!request.FileIsTooLarge)
                return false;

            await _tgClientWrapper.UpdateMessageAsync(
                "File is to large to be sent in telegram. "
                + "Trying to get tracks using silence detection.",
                cancellationToken);

            files = _mp3SplitWrapper.SplitBySilenceAsync(file, cancellationToken);

            if (await SendNewTrackRequests(files, cancellationToken))
                return true;

            await _tgClientWrapper.UpdateMessageAsync(
                "Silence detection failed. Track would be sent as multiple files.",
                cancellationToken);

            var equalPartsCount = (int)Math.Round(
                file.Length / (decimal)_botOptions.CurrentValue.MaxFileBytesCount,
                MidpointRounding.ToPositiveInfinity);

            files = _mp3SplitWrapper.SplitInEqualPartsAsync(
                file,
                equalPartsCount,
                cancellationToken);
            if (!await SendNewTrackRequests(files, cancellationToken))
                throw new InvalidOperationException("Couldn't split file in equal parts");

            return true;
        }


        private async Task<TracksList?> GetTrackListFromDescriptionOrNull(
            IFileInfo file,
            CancellationToken cancellationToken)
        {
            var descriptionFile = _descriptionService.GetFileOrNull(file);

            if (descriptionFile == null)
                return null;

            var description = await descriptionFile.GetTextAsync(cancellationToken);

            if (string.IsNullOrEmpty(description))
                return null;

            var trackList = _trackListParser.Parse(description)
                .ToArray();
            if (trackList.Length < 2)
                return null;

            return new(trackList);
        }

        private async Task<bool> SendNewTrackRequests(
            IAsyncEnumerable<IFileInfo> files,
            CancellationToken cancellationToken)
        {
            var result = false;
            await foreach (var file in files.WithCancellation(cancellationToken))
            {
                var res = await _mediator
                    .Send<NewTrackHandler.Request, bool>(
                        new NewTrackHandler.Request(file, SkipSplit: true),
                        cancellationToken);

                if (!res)
                    return false;

                result = true;
            }

            return result;
        }

        public record Request(IFileInfo File, bool FileIsTooLarge = false) : IRequest<bool>;
    }
}
