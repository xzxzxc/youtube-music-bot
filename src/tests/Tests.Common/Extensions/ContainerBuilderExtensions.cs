using System;
using System.Linq.Expressions;
using Autofac;
using Autofac.Core;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;

namespace YoutubeMusicBot.IntegrationTests.Common.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterDefaultOptions<TOptions>(
            this ContainerBuilder builder)
            where TOptions : class, new() =>
            builder.RegisterOptions(new TOptions());

        public static ContainerBuilder RegisterOptions<TOptions>(
            this ContainerBuilder builder,
            TOptions options)
            where TOptions : class
        {
            var redisOptionsMock = new Mock<IOptionsMonitor<TOptions>>
            {
                DefaultValue = DefaultValue.Mock,
            };
            redisOptionsMock
                .SetupGet(m => m.CurrentValue)
                .Returns(options);
            builder.RegisterMock(redisOptionsMock);

            return builder;
        }

        public static ContainerBuilder RegisterMockOf<TService>(
            this ContainerBuilder builder,
            Expression<Func<TService, bool>> predicate,
            MockBehavior behavior = MockBehavior.Default)
            where TService : class
        {
            builder.RegisterInstance(Mock.Of<TService>(predicate, behavior));

            return builder;
        }

        public static ContainerBuilder RegisterModules(
            this ContainerBuilder builder,
            params IModule[] modules)
        {
            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }

            return builder;
        }
    }
}
