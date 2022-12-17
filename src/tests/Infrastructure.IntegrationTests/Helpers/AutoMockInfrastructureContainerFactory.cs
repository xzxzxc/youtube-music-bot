using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            foreach (var hostedService in
                     container.Container.Resolve<IEnumerable<IHostedService>>())
            {
                await hostedService.StartAsync(default);
            }

            return container;
        }
    }
}
