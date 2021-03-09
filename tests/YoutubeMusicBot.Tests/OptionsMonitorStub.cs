using System;
using Microsoft.Extensions.Options;

namespace YoutubeMusicBot.Tests
{
	class OptionsMonitorStub<T> : IOptionsMonitor<T>
		where T : new()
	{
		public T Get(string name)
		{
			throw new NotImplementedException();
		}

		public IDisposable OnChange(Action<T, string> listener)
		{
			throw new NotImplementedException();
		}

		public T CurrentValue { get; } = new T();
	}
}
