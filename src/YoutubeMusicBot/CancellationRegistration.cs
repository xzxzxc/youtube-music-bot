using System;
using System.Collections.Concurrent;
using System.Threading;
using YoutubeMusicBot.Interfaces;

namespace YoutubeMusicBot
{
    internal class CancellationRegistration : ICancellationRegistration
    {
        private readonly ICallbackFactory _callbackFactory;

        public CancellationRegistration(ICallbackFactory callbackFactory)
        {
            _callbackFactory = callbackFactory;
        }

        private readonly ConcurrentDictionary<string, CancellationProvider> _providers = new();

        public ICancellationProvider GetProvider(string id) =>
            _providers[id];

        public ICancellationProvider RegisterNewProvider()
        {
            var callbackData = _callbackFactory.CreateDataForCancellation();

            var provider = new CancellationProvider(callbackData, this);
            _providers.AddOrUpdate(
                callbackData,
                _ => provider,
                (_, _) => throw new InvalidOperationException("Hash collision!"));

            return provider;
        }

        private void RemoveProvider(string id) =>
            _providers.TryRemove(id, out _);

        private class CancellationProvider : ICancellationProvider
        {
            private readonly CancellationRegistration _cancellationRegistration;
            private readonly CancellationTokenSource _source;

            public CancellationProvider(
                string callbackData,
                CancellationRegistration cancellationRegistration)
            {
                _cancellationRegistration = cancellationRegistration;
                CallbackData = callbackData;
                _source = new CancellationTokenSource();
            }


            public string CallbackData { get; }

            public CancellationToken Token => _source.Token;

            public void Cancel() =>
                _source.Cancel();

            public void Dispose()
            {
                _cancellationRegistration.RemoveProvider(CallbackData);
            }
        }
    }
}
