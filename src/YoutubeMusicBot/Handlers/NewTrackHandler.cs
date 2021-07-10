using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Models;
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
        private readonly IOptionsMonitor<FeatureOptions> _featureOptions;
        private readonly IMediator _mediator;
        private readonly MessageContext _messageContext;

        private readonly Regex _silenceDetectionRegex = new(
            @"\(Selected (\d+) tracks\)",
            RegexOptions.Compiled);

        private FileInfo? _file;

        public NewTrackHandler(
            ITgClientWrapper tgClientWrapper,
            IOptionsMonitor<BotOptions> botOptions,
            IOptionsMonitor<FeatureOptions> featureOptions,
            IMediator mediator,
            MessageContext messageContext)
        {
            _tgClientWrapper = tgClientWrapper;
            _botOptions = botOptions;
            _featureOptions = featureOptions;
            _mediator = mediator;
            _messageContext = messageContext;
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
                    new TrySplitHandler.Request(request.File),
                    cancellationToken);

                if (split)
                    return Unit.Value;
            }

            if (file.Length > _botOptions.CurrentValue.MaxFileBytesCount)
            {
                if (_featureOptions.CurrentValue.SplitButtonsEnabled)
                {
                    var equalPartsCount =
                        file.Length / _botOptions.CurrentValue.MaxFileBytesCount + 1;
                    int? silenceDetectedPartsCount = null;
                    await _mediator.Send(
                        new RunProcessHandler.Request(
                            "mp3splt",
                            request.File.DirectoryName
                            ?? throw new InvalidOperationException(
                                "File directory doesn't exists."),
                            async (line, _) =>
                            {
                                var match = _silenceDetectionRegex.Match(line);
                                if (match.Success)
                                    silenceDetectedPartsCount = int.Parse(match.Groups[1].Value);
                            },
                            "-s",
                            "-P",
                            request.File.Name));
                    var buttonCollection = new List<InlineButton>
                    {
                        new($"Split into {equalPartsCount} equal parts.", string.Empty)
                    };
                    if (silenceDetectedPartsCount.HasValue)
                    {
                        buttonCollection.Add(
                            new InlineButton(
                                $"Split into {silenceDetectedPartsCount} parts using silence detection.",
                                string.Empty));
                    }

                    await _tgClientWrapper.SendMessageAsync(
                        $"\"{_messageContext.Title ?? file.Name}\" is too large to be sent in telegram."
                        + " But I could split it.",
                        new InlineButtonCollection(buttonCollection),
                        cancellationToken);
                }
                else
                {
                    await _tgClientWrapper.SendMessageAsync(
                        "File is to large to be sent in telegram :(",
                        cancellationToken: cancellationToken);
                }

                return Unit.Value;
            }

            await _tgClientWrapper.SendAudioAsync(file, cancellationToken);

            return Unit.Value;
        }

        public void Dispose()
        {
            _file?.Delete();
        }

        public record Request(FileInfo File, bool TrySplit = true) : IRequest;
    }
}
