using System;
using System.Collections.Concurrent;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	internal class TgClientsHolder
	{
		private readonly Func<ChatContext, TgClientWrapper> _watcherFactory;
		private readonly ConcurrentDictionary<
			ChatContext,
			ITgClientWrapper> _cache = new();

		public TgClientsHolder(
			Func<ChatContext, TgClientWrapper> watcherFactory)
		{
			_watcherFactory = watcherFactory;
		}

		public ITgClientWrapper Get(ChatContext key) =>
			_cache.GetOrAdd(key, _watcherFactory);
	}
}
