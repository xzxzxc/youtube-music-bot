using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    public class NewTrackHandler :
        IRequestHandler<NewTrackHandler.Request, Unit>,
        IDisposable
    {
        private readonly ITgClientWrapper _tgClientWrapper;
        private readonly IOptionsMonitor<BotOptions> _botOptions;
        private readonly IOptionsMonitor<FeatureOptions> _featureOptions;
        private readonly IMediator _mediator;
        private readonly IProcessRunner _processRunner;
        private readonly IDescriptionService _descriptionService;
        private readonly MessageContext _messageContext;

        private readonly Regex _silenceDetectionRegex = new(
            @"\(Selected (\d+) tracks\)",
            RegexOptions.Compiled);

        private IFileInfo? _file;
        private IFileInfo? _descriptionFile;

        public NewTrackHandler(
            ITgClientWrapper tgClientWrapper,
            IOptionsMonitor<BotOptions> botOptions,
            IOptionsMonitor<FeatureOptions> featureOptions,
            IMediator mediator,
            IProcessRunner processRunner,
            IDescriptionService descriptionService,
            MessageContext messageContext)
        {
            _tgClientWrapper = tgClientWrapper;
            _botOptions = botOptions;
            _featureOptions = featureOptions;
            _mediator = mediator;
            _processRunner = processRunner;
            _descriptionService = descriptionService;
            _messageContext = messageContext;
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

            if (request.TrySplit)
            {
                var split = await _mediator.Send(
                    new TrySplitHandler.Request(file),
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
                    var fileDirectory = file.DirectoryName
                        ?? throw new InvalidOperationException(
                            "File directory doesn't exists.");
                    await foreach (var line in _processRunner.RunAsync(
                        new ProcessRunner.Request(
                            ProcessName: "mp3splt",
                            WorkingDirectory: fileDirectory,
                            "-s",
                            "-P",
                            file.Name),
                        cancellationToken))
                    {
                        var match = _silenceDetectionRegex.Match(line);
                        if (match.Success)
                        {
                            silenceDetectedPartsCount = int.Parse(match.Groups[1].Value);
                            break;
                        }
                    }

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
            if (_file?.Exists ?? false)
                _file.Delete();

            if (_descriptionFile?.Exists ?? false)
                _descriptionFile.Delete();
        }

        public record Request(IFileInfo File, bool TrySplit = true) : IRequest;
    }
}