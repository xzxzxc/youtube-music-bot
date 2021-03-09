using System;
using System.IO;
using System.Threading.Tasks;

namespace YoutubeMusicBot
{
	internal class FileWrapper : IFileWrapper
	{
		private readonly FileInfo _fileInfo;
		private readonly Func<ValueTask>? _onDispose;

		public FileWrapper(
			FileInfo fileInfo,
			Func<ValueTask>? onDispose = null)
		{
			_fileInfo = fileInfo;
			_onDispose = onDispose;
		}

		public Stream Stream => _fileInfo.OpenRead();

		public string Name => _fileInfo.Name;

		public async ValueTask DisposeAsync()
		{
			await (_onDispose?.Invoke() ?? default);
		}
	}
}