using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;

namespace YoutubeMusicBot.Infrastructure.IntegrationTest.Helpers
{
    public static class AutoMockInfrastructureContainerFactory
    {
        public static async ValueTask<AutoMock> Create(
            Action<ContainerBuilder>? beforeBuild = null,
            bool initialize = false,
            bool verifyAllMocks = true)
        {
            var container = AutoMockContainerFactory.Create(
                builder =>
                {
                    builder.RegisterModules(new CommonModule());
                    builder.RegisterGeneric(typeof(ThrowExceptionLogger<>)).As(typeof(ILogger<>));

                    beforeBuild?.Invoke(builder);
                },
                verifyAllMocks);

            if (initialize)
            {
                foreach (var initializable in container.Container
                    .Resolve<IEnumerable<IInitializable>>()
                    .OrderBy(e => e.Order))
                {
                    await initializable.Initialize();
                }
            }

            return container;
        }
    }
}
