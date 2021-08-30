using System;
using System.Collections.Generic;
using System.Linq;
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
            ReadOnlySpan<byte> bytes = Convert.FromBase64String(callbackData);

            var aggregateId = BitConverter.ToInt64(bytes[1..9]);
            var cacheKey = BitConverter.ToInt32(bytes[9..13]);
            return bytes[0] switch
            {
                1 => CreateCancelResult(
                    aggregateId,
                    _aggregateTypes[cacheKey]),
                _ => throw new InvalidOperationException(
                    $"Unknown callback data identifier. "
                    + $"Bytes: {string.Join(",", bytes.ToArray())}")
            };
        }


        public string CreateForCancel<TAggregate>(EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>
        {
            Span<byte> bytes = stackalloc byte[13];
            bytes[0] = 1;
            BitConverter.TryWriteBytes(bytes[1..9], @event.AggregateId);
            var cacheKey = GetCacheKey(typeof(TAggregate));
            BitConverter.TryWriteBytes(bytes[9..13], cacheKey);

            return Convert.ToBase64String(bytes);
        }

        private static ICancelResult CreateCancelResult(long aggregateId, Type aggregateType) =>
            (ICancelResult)Activator.CreateInstance(
                typeof(CancelResult<>).MakeGenericType(aggregateType),
                aggregateId)!;

        private static int GetCacheKey(Type t) =>
            (t.FullName ?? t.Name).GetDeterministicHashCode();
    }
}
