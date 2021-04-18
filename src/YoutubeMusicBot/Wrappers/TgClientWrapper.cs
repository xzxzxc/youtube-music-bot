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
		private readonly ChatContext _chatContext;
		private readonly ITelegramBotClient _telegramBotClient;

		public TgClientWrapper(
			ChatContext chatContext,
			ITelegramBotClient telegramBotClient)
		{
			_chatContext = chatContext;
			_telegramBotClient = telegramBotClient;
		}

		public async Task<Message> SendAudioAsync(
			FileInfo audio)
		{
			// TODO: add retry policy
			// TODO: add flood control handling
			await using var fileStream = audio.OpenRead();
			return await _telegramBotClient.SendAudioAsync(
				_chatContext.Id,
				new InputMedia(
					fileStream,
					audio.Name));
		}
	}
}
