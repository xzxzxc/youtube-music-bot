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
        IRequestHandler<TrySplitHandler.Request, bool>,
        IDisposable
    {
        private readonly IMp3SplitWrapper _mp3SplitWrapper;
        private readonly ITrackListParser _trackListParser;
        private FileInfo? _descriptionFile;

        public TrySplitHandler(
            IMp3SplitWrapper mp3SplitWrapper,
            ITrackListParser trackListParser)
        {
            _mp3SplitWrapper = mp3SplitWrapper;
            _trackListParser = trackListParser;
        }

        public async Task<bool> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var file = request.File;
            var directoryName = file.DirectoryName
                ?? throw new ArgumentOutOfRangeException(
                    nameof(request),
                    request,
                    "File has no dictionary ");

            var descriptionFileName = Path.ChangeExtension(file.Name, "description");
            var descriptionFile = new FileInfo(Path.Join(directoryName, descriptionFileName));

            if (!descriptionFile.Exists)
                return false;

            _descriptionFile = descriptionFile;

            var description = await File.ReadAllTextAsync(
                descriptionFile.FullName,
                cancellationToken);

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

        public void Dispose()
        {
            _descriptionFile?.Delete();
        }

        public record Request(IFileInfo File) : IRequest<bool>;
    }
}
