using Telegram.Bot.Types;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
	public static class TgClientModelsExtensions
	{
		public static ChatContext ToContext(this Chat chat) =>
			new(chat.Id);

		public static MessageContext ToContext(this Message message) =>
			new(message.Chat.ToContext());
	}
}
