using Telegram.Bot.Types;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
	public static class TgClientModelsExtensions
	{
		public static ChatContext ToContext(this Chat chat) =>
			new(chat.Id);
	}
}
