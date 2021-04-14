using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac.Features.Indexed;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
	public class TrackFilesWatcherIndex : IIndex<ChatContext, ITrackFilesWatcher>
	{
		private readonly Func<ChatContext, Lazy<ITrackFilesWatcher>> _watcherFactory;
		private readonly ConcurrentDictionary<
			ChatContext,
			Lazy<ITrackFilesWatcher>> _cache = new();

		public TrackFilesWatcherIndex(
			Func<ChatContext, Lazy<ITrackFilesWatcher>> watcherFactory)
		{
			_watcherFactory = watcherFactory;
		}

		public bool TryGetValue(ChatContext key, out ITrackFilesWatcher value)
		{
			value = _cache.GetOrAdd(key, _watcherFactory).Value;

			return true;
		}

		public ITrackFilesWatcher this[ChatContext key] =>
			TryGetValue(key, out var res)
				? res
				: throw new KeyNotFoundException();
	}
}
