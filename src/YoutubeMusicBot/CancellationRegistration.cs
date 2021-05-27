using System;
using System.Collections.Concurrent;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    internal class CancellationRegistration : ICancellationRegistration
    {
        private readonly ConcurrentDictionary<string, CancellationProvider> _providers = new();

        public CancellationProvider GetProvider(string cancellationProviderId) =>
            _providers[cancellationProviderId];

        public CancellationProvider RegisterNewProvider()
        {
            var id = Guid.NewGuid().ToString("N")[..^2];
            var provider = new CancellationProvider(id, () => _providers.TryRemove(id, out _));
            _providers.AddOrUpdate(
                provider.Id,
                _ => provider,
                (_, _) => throw new InvalidOperationException("Hash collision!"));

            return provider;
        }
    }
}
