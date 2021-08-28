using System.IO;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Infrastructure
{
	internal class CacheFolder : ICacheFolder
	{
		public CacheFolder(
			MessageContext messageContext,
			IOptionsMonitor<FileSystemOptions> downloadOptions)
		{
			var chatFolderPath = Value = Path.Join(
				downloadOptions.CurrentValue.TempFolderPath,
				$"{messageContext.UserMessage.Chat.Id}");

			Directory.CreateDirectory(chatFolderPath);
		}

		public string Value { get; }
	}
}
