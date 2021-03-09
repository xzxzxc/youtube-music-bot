using System;
using System.IO;

namespace YoutubeMusicBot
{
	interface IFileWrapper : IAsyncDisposable
	{
		Stream Stream { get; }

		string Name { get; }
	}
}