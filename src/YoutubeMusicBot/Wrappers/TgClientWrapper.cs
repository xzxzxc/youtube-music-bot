using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Wrappers
{
	internal class TgClientWrapper : ITgClientWrapper
	{
		private readonly MessageContext _context;
		private readonly ITelegramBotClient _telegramBotClient;

		public TgClientWrapper(
			MessageContext context,
			ITelegramBotClient telegramBotClient)
		{
			_context = context;
			_telegramBotClient = telegramBotClient;
		}

		public async Task<Message> SendAudioAsync(
			FileInfo audio,
			CancellationToken cancellationToken = default)
		{
			// TODO: add retry policy
			await using var fileStream = audio.OpenRead();
			var inputMedia = new InputMedia(
				fileStream,
				audio.Name);
			return await Ivoke(
				() => _telegramBotClient.SendAudioAsync(
					_context.Chat.Id,
					inputMedia,
					cancellationToken: cancellationToken));
		}

		public async Task<Message> SendMessageAsync(
			string message,
			CancellationToken cancellationToken = default) =>
			await Ivoke(
				() => _telegramBotClient.SendTextMessageAsync(
					_context.Chat.Id,
					message,
					cancellationToken: cancellationToken));

		private async Task<Message> Ivoke(Func<Task<Message>> action)
		{
			try
			{
				return await action();
			}
			catch (ApiRequestException ex)
				when (ex.Parameters?.RetryAfter != null)
			{
				var time = TimeSpan.FromSeconds(ex.Parameters.RetryAfter.Value);
				await Task.Delay(time);
				return await Ivoke(action);
			}
		}
	}
}
