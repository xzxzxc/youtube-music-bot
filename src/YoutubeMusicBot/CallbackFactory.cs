using System;
using YoutubeMusicBot.Interfaces;

namespace YoutubeMusicBot
{
    internal class CallbackFactory : ICallbackFactory
    {
        private const int MaxDataBytesCount = 64;
        private const byte CancellationPrefix = 1;
        private static Random _random = new();

        public CallbackAction GetActionFromData(byte[] callbackData) =>
            callbackData[0] switch
            {
                CancellationPrefix => CallbackAction.Cancel,
                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(callbackData),
                        callbackData,
                        "Callback data couldn't be parsed"),
            };

        public byte[] CreateDataForCancellation()
        {
            var bytes = new byte[MaxDataBytesCount];
            bytes[0] = CancellationPrefix;
            var bytesSpan = new Span<byte>(bytes, 1, MaxDataBytesCount - 1);
            _random.NextBytes(bytesSpan);

            return bytes;
        }
    }
}
