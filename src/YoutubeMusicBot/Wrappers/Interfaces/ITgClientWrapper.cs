﻿using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Wrappers.Interfaces
{
	public interface ITgClientWrapper
	{
		Task<Message> SendAudioAsync(
			ChatContext chat,
			FileInfo audio);
	}
}