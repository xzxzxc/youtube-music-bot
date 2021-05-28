using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace YoutubeMusicBot.Models
{
    public class CancellationProvider : IDisposable
    {
        private readonly Action _dispose;
        private readonly CancellationTokenSource _source;

        public CancellationProvider(string id, Action dispose)
        {
            Id = id;
            _dispose = dispose;
            _source = new CancellationTokenSource();
        }

        public string Id { get; }

        // TODO: move to CancellationCallbackFactory
        public string Str => $"c_{Id}";

        public CancellationToken Token => _source.Token;

        public void Cancel() =>
            _source.Cancel();

        public void Dispose() =>
            _dispose();

        // TODO: move to CancellationCallbackFactory
        public static bool TryGetId(string str, [NotNullWhen(returnValue: true)] out string? id)
        {
            var res = str.StartsWith("c_");
            id = res
                ? str.Substring(2)
                : null;

            return res;
        }
    }
}
