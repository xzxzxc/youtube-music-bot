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
            // TODO: move to CancellationCallbackFactory and create wrapper with dispose functionality
            var id = Guid.NewGuid().ToString("N")[..^4];
            var provider = new CancellationProvider(id, () => _providers.TryRemove(id, out _));
            _providers.AddOrUpdate(
                provider.Id,
                _ => provider,
                (_, _) => throw new InvalidOperationException("Hash collision!"));

            return provider;
        }
    }
}
