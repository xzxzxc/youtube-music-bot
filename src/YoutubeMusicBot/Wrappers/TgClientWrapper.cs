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
		private readonly MessageContext _messageContext;
		private readonly ITelegramBotClient _telegramBotClient;

		public TgClientWrapper(
			MessageContext messageContext,
			ITelegramBotClient telegramBotClient)
		{
			_messageContext = messageContext;
			_telegramBotClient = telegramBotClient;
		}

		public async Task<Message> SendAudioAsync(
			FileInfo audio)
		{
			// TODO: add flood control handling
			await using var fileStream = audio.OpenRead();
			return await _telegramBotClient.SendAudioAsync(
				_messageContext.Chat.Id,
				new InputMedia(
					fileStream,
					audio.Name));
		}
	}
}
