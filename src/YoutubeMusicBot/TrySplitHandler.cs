using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	internal class TrySplitHandler :
		IRequestHandler<TrySplitHandler.Notification, bool>,
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
			Notification notification,
			CancellationToken cancellationToken)
		{
			var file = notification.File;
			var descriptionFile = new FileInfo(
				Path.Join(
					file.DirectoryName ?? throw new InvalidOperationException(), // TODO
					$"{Path.GetFileNameWithoutExtension(file.Name)}.description"));

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
			if (trackList.Length == 0)
				return false;

			await _mp3SplitWrapper.SplitAsync(
				notification.File,
				trackList,
				cancellationToken);

			return true;
		}

		public void Dispose()
		{
			_descriptionFile?.Delete();
		}

		internal record Notification(
				ChatContext Chat,
				FileInfo File)
			: IRequest<bool>
		{
		}
	}
}
