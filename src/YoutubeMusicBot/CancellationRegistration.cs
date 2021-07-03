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

        private readonly ConcurrentDictionary<long, CancellationProvider> _providers = new();

        public ICancellationProvider GetProvider(byte[] id) =>
            _providers[GetHash(id)];

        public ICancellationProvider RegisterNewProvider()
        {
            var callbackData = _callbackFactory.CreateDataForCancellation();

            var provider = new CancellationProvider(callbackData, this);
            _providers.AddOrUpdate(
                GetHash(callbackData),
                _ => provider,
                (_, _) => throw new InvalidOperationException("Hash collision!"));

            return provider;
        }

        private void RemoveProvider(byte[] id) =>
            _providers.TryRemove(GetHash(id), out _);

        private static long GetHash(byte[] bytes)
        {
            var res = 0L;
            for (var i = 0; i < bytes.Length; i++)
            {
                const int bytesInLong = sizeof(long) / sizeof(byte);
                var bytePositionInLong = i % bytesInLong;

                res = res ^ (((long)bytes[i]) << bytePositionInLong * sizeof(byte));
            }

            return res;
        }

        private class CancellationProvider : ICancellationProvider
        {
            private readonly CancellationRegistration _cancellationRegistration;
            private readonly CancellationTokenSource _source;

            public CancellationProvider(
                byte[] callbackData,
                CancellationRegistration cancellationRegistration)
            {
                _cancellationRegistration = cancellationRegistration;
                CallbackData = callbackData;
                _source = new CancellationTokenSource();
            }


            public byte[] CallbackData { get; }

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
