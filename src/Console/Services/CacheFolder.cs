using System.IO;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Options;

namespace YoutubeMusicBot.Console.Services
{
	internal class CacheFolder : ICacheFolder
	{
		public CacheFolder(
			MessageContext messageContext,
			IOptionsMonitor<DownloadOptions> downloadOptions)
		{
			var chatFolderPath = Value = Path.Join(
				downloadOptions.CurrentValue.CacheFilesFolderPath,
				$"{messageContext.UserMessage.Chat.Id}");

			Directory.CreateDirectory(chatFolderPath);
		}

		public string Value { get; }
	}
}
