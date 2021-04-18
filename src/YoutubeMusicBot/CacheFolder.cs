using System.IO;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;

namespace YoutubeMusicBot
{
	internal class CacheFolder : ICacheFolder
	{
		public CacheFolder(
			MessageContext messageContext,
			IOptionsMonitor<DownloadOptions> downloadOptions)
		{
			var chatFolderPath = Value = Path.Join(
				downloadOptions.CurrentValue.CacheFilesFolderPath,
				$"{messageContext.Chat.Id}");

			Directory.CreateDirectory(chatFolderPath);
		}

		public string Value { get; }
	}
}
