using System;
using Moq;

namespace YoutubeMusicBot.Tests.Common
{
    public class LazyCapture<T>
    {
        private T? _value;

        public LazyCapture()
        {
            Match = new CaptureMatch<T>(m => _value = m);
        }

        public CaptureMatch<T> Match { get; }

        public T Value =>
            _value
            ?? throw new InvalidOperationException(
                "Mock with setup wasn't called, or null was passed as argument.");
    }
}
