using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Options;
using YoutubeMusicBot.Console.Wrappers.Interfaces;

namespace YoutubeMusicBot.Console.Handlers
{
    public class TrySplitHandler :
        IRequestHandler<TrySplitHandler.Request, bool>
    {
        private readonly IMp3SplitWrapper _mp3SplitWrapper;
        private readonly ITrackListParser _trackListParser;
        private readonly IDescriptionService _descriptionService;
        private readonly IMediator _mediator;
        private readonly IOptionsMonitor<BotOptions> _botOptions;

        public TrySplitHandler(
            IMp3SplitWrapper mp3SplitWrapper,
            ITrackListParser trackListParser,
            IDescriptionService descriptionService,
            IMediator mediator,
            IOptionsMonitor<BotOptions> botOptions)
        {
            _mp3SplitWrapper = mp3SplitWrapper;
            _trackListParser = trackListParser;
            _descriptionService = descriptionService;
            _mediator = mediator;
            _botOptions = botOptions;
        }

        public async Task<bool> Handle(
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

            if (!request.ForceSplit)
                return false;

            files = _mp3SplitWrapper.SplitBySilenceAsync(file, cancellationToken);

            if (await SendNewTrackRequests(files, cancellationToken))
                return true;

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
            var res = false;
            await foreach (var file in files.WithCancellation(cancellationToken))
            {
                await _mediator
                    .Send(
                        new NewTrackHandler.Request(file, SkipSplit: true),
                        cancellationToken);
                res = true;
            }

            return res;
        }

        public record Request(IFileInfo File, bool ForceSplit = false) : IRequest<bool>;
    }
}
