using System;
using System.Text;
using YoutubeMusicBot.Interfaces;

namespace YoutubeMusicBot
{
    internal class CallbackFactory : ICallbackFactory
    {
        private const int MaxDataBytesCount = 64;
        private const char CancellationPrefix = 'c';

        public CallbackAction GetActionFromData(string callbackData) =>
            callbackData[0] switch
            {
                CancellationPrefix => CallbackAction.Cancel,
                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(callbackData),
                        callbackData,
                        "Callback data couldn't be parsed"),
            };

        public string CreateDataForCancellation()
        {
            var prefix = CancellationPrefix.ToString();
            var randomStr = Guid.NewGuid().ToString("N");

            var prefixBytesCount = Encoding.Unicode.GetByteCount(prefix);
            var randomBytesCount = Encoding.Unicode.GetByteCount(randomStr);
            var randomStrBytesOverflow = MaxDataBytesCount - prefixBytesCount - randomBytesCount;

            if (randomStrBytesOverflow < 0)
            {
                var randomStrBytesAllowedLength =
                    randomStr.Length
                    * (randomBytesCount + randomStrBytesOverflow)
                    / randomBytesCount;

                randomStr = randomStr.Substring(0, randomStrBytesAllowedLength);
            }

            return $"{prefix}{randomStr}";
        }
    }
}
