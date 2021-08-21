using System;
using System.Collections.Concurrent;
using System.Threading;
using YoutubeMusicBot.Console.Interfaces;

namespace YoutubeMusicBot.Console.Services
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

        public ICancellationProvider RegisterNewProvider(CancellationToken initialToken)
        {
            var callbackData = _callbackFactory.CreateDataForCancellation();

            var provider = new CancellationProvider(callbackData, initialToken, this);
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
            private readonly CancellationTokenSource _cancelSource;
            private readonly CancellationTokenSource _aggregateSource;

            public CancellationProvider(
                string callbackData,
                CancellationToken initialToken,
                CancellationRegistration cancellationRegistration)
            {
                _cancellationRegistration = cancellationRegistration;
                CallbackData = callbackData;
                _cancelSource = new CancellationTokenSource();
                _aggregateSource = CancellationTokenSource.CreateLinkedTokenSource(
                    initialToken,
                    _cancelSource.Token);
            }


            public string CallbackData { get; }

            public CancellationToken Token =>
                _aggregateSource.Token;

            public void Cancel() =>
                _cancelSource.Cancel();

            public void Dispose() =>
                _cancellationRegistration.RemoveProvider(CallbackData);
        }
    }
}
