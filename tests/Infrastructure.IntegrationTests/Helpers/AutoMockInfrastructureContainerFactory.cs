using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Tests.Common;

namespace Infrastructure.IntegrationTests.Helpers
{
    public static class AutoMockInfrastructureContainerFactory
    {
        public static async ValueTask<AutoMock> Create(Action<ContainerBuilder>? beforeBuild = null)
        {
            var container = beforeBuild == null
                ? AutoMockContainerFactory.Create()
                : AutoMockContainerFactory.Create(beforeBuild);

            foreach (var initializable in container.Container.Resolve<IEnumerable<IInitializable>>()
                .OrderBy(e => e.Order))
            {
                await initializable.Initialize();
            }

            return container;
        }
    }
}
