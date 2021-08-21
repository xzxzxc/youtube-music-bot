using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq.Sequences;
using NUnit.Framework;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Tests.Common;

namespace Application.IntegrationTests
{
    [SetUpFixture]
    public class CommonFixture
    {
        public static AutoMock Container { get; private set; } = null!;

        public static IFixture FixtureInstance { get; private set; } = null!;

        [OneTimeSetUp]
        public static async Task OneTimeSetUp()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            FixtureInstance = AutoFixtureFactory.Create();
            Container = AutoMockContainerFactory.Create(
                (mockRepository, builder) =>
                {
                    builder.RegisterModule(new ServicesModule());
                    builder.RegisterModule(new MessageHandlerModule());
                    builder.RegisterModule(new DbContextModule("tests_temp_folder"));
                });

            await EnsureDatabaseAsync();
        }

        private static async Task EnsureDatabaseAsync()
        {
            var identityDbContext = Container.Create<ApplicationDbContext>();
            await identityDbContext.Database.MigrateAsync();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            Container.Dispose();
        }
    }
}
