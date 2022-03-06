using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.IntegrationTests.Common;
using BindingFlags = System.Reflection.BindingFlags;

namespace YoutubeMusicBot.Domain.UnitTests
{
    public class AggregateBaseTests : BaseParallelizableTest
    {
        [Test]
        public void AllAggregatesShouldHavePrivateConstructors()
        {
            var assembly = typeof(AggregateBase<>).Assembly;
            var aggregateTypes = assembly
                .GetTypes()
                .Where(
                    t => t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(AggregateBase<>));

            aggregateTypes.Should()
                .OnlyContain(
                    c => c.GetConstructor(
                            BindingFlags.NonPublic | BindingFlags.Instance,
                            null,
                            Type.EmptyTypes,
                            null)
                        != null);
        }
    }
}
