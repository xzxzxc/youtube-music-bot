using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Serilog;
using Serilog.Sinks.InMemory;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Application.IntegrationTests
{
    [SetUpFixture]
    public class CommonFixture
    {
        private static IHost _host = null!;

        public static DirectoryInfo CacheFolder => new("cache_folder");

        public static Mock<ITgClientWrapper> TgClientMock { get; private set; } = null!;

        public static ILifetimeScope RootScope { get; private set; } = null!;

        public static IFixture FixtureInstance { get; private set; } = null!;

        [OneTimeSetUp]
        public static async Task OneTimeSetUp()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            FixtureInstance = AutoFixtureFactory.Create();
            TgClientMock = new Mock<ITgClientWrapper>
            {
                DefaultValueProvider = new AutoFixtureValueProvider(FixtureInstance)
            };
            _host = Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, b) =>
                    {
                        b.RegisterMock(TgClientMock);
                        b.RegisterOptions(new DownloadOptions
                        {
                            CacheFilesFolderPath = CacheFolder.FullName,
                        });
                    })
                .UseSerilog(new LoggerConfiguration().WriteTo.InMemory().CreateLogger())
                .Build();

            RootScope = _host.Services.GetRequiredService<ILifetimeScope>();
            await EnsureDatabaseAsync();

            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        private static async Task EnsureDatabaseAsync()
        {
            var identityDbContext = RootScope.Resolve<ApplicationDbContext>();
            await identityDbContext.Database.MigrateAsync();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            _host.Dispose();

            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }
    }
}
