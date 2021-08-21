using System;
using YoutubeMusicBot.Console.Enums;
using YoutubeMusicBot.Console.Interfaces;

namespace YoutubeMusicBot.Console.Services
{
    public class CallbackFactory : ICallbackFactory
    {
        private const char CancellationPrefix = 'c';
        private static readonly Random _random = new();

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

        public string CreateDataForCancellation() =>
            $"{CancellationPrefix}{_random.Next()}";
    }
}
