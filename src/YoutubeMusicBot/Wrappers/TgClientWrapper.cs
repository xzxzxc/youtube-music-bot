using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class TgClientWrapper : ITgClientWrapper
	{
		private readonly ITelegramBotClient _telegramBotClient;

		public TgClientWrapper(
			ITelegramBotClient telegramBotClient)
		{
			_telegramBotClient = telegramBotClient;
		}

		public async Task<Message> SendAudioAsync(
			ChatContext chat,
			FileInfo audio)
		{
			// TODO: add flood control handling
			await using var fileStream = audio.OpenRead();
			return await _telegramBotClient.SendAudioAsync(
				chat.Id,
				new InputMedia(
					fileStream,
					audio.Name));
		}
	}
}
