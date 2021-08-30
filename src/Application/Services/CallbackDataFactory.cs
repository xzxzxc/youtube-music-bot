using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Services
{
    public class CallbackDataFactory : ICallbackDataFactory
    {
        private readonly Dictionary<int, Type> _aggregateTypes;

        public CallbackDataFactory(IEnumerable<Type> aggregateTypes)
        {
            _aggregateTypes = aggregateTypes
                .ToDictionary(GetCacheKey);
        }

        public ICallbackResult Parse(string callbackData)
        {
            Span<byte> stackSpan = stackalloc byte[64];
            Encoding.Unicode.GetBytes(callbackData, stackSpan);

            var aggregateId = BitConverter.ToInt64(stackSpan[1..9]);
            var cacheKey = BitConverter.ToInt32(stackSpan[9..13]);
            return stackSpan[0] switch
            {
                1 => CreateCancelResult(
                    aggregateId,
                    _aggregateTypes[cacheKey]),
                _ => throw new InvalidOperationException(
                    $"Unknown callback data identifier. "
                    + $"Bytes: {string.Join(",", stackSpan.ToArray())}")
            };
        }


        public string CreateForCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>
        {
            Span<byte> stackSpan = stackalloc byte[64];
            stackSpan[0] = 1;
            BitConverter.TryWriteBytes(stackSpan[1..9], @event.AggregateId);
            var cacheKey = GetCacheKey(typeof(TAggregate));
            BitConverter.TryWriteBytes(stackSpan[9..13], cacheKey);

            return Encoding.Unicode.GetString(stackSpan);
        }

        private static ICancelResult CreateCancelResult(long aggregateId, Type aggregateType) =>
            (ICancelResult)Activator.CreateInstance(
                typeof(CancelResult<>).MakeGenericType(aggregateType),
                aggregateId)!;

        private static int GetCacheKey(Type t) =>
            (t.FullName ?? t.Name).GetDeterministicHashCode();
    }
}
