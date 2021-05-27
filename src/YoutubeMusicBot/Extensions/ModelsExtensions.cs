using Telegram.Bot.Types.ReplyMarkups;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
	public static class ModelsExtensions
	{
		public static IReplyMarkup ToMarkup(this InlineButton button) =>
			new InlineKeyboardMarkup(
				new InlineKeyboardButton
				{
					Text = button.Text,
					CallbackData = button.CallbackData,
				});
	}
}
