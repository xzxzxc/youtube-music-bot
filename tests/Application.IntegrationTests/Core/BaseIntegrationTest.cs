using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq.Sequences;
using NUnit.Framework;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;

namespace YoutubeMusicBot.Application.IntegrationTests.Core
{
    public abstract class BaseIntegrationTest : BaseParallelizableWithTempFolderTest
    {
        static BaseIntegrationTest()
        {
            Sequence.ContextMode = SequenceContextMode.Async;
        }

        public BaseIntegrationTest()
        {
            FixtureInstance = AutoFixtureFactory.Create();
            Container = AutoMockContainerFactory.Create(
                (mockRepository, builder) =>
                {
                    builder.RegisterApplicationModules();
                    builder.RegisterModule(
                        new DbContextModule(TempFolder.FullName, enableSensitiveLogin: true));
                });
        }

        protected IFixture FixtureInstance { get; }

        protected AutoMock Container { get; }

        public override async ValueTask SetUp()
        {
            await base.SetUp();

            await EnsureDatabaseAsync();
        }

        [TearDown]
        public override async ValueTask TearDown()
        {
            await base.SetUp();

            Container.Dispose();
        }

        protected Task AddToDb<T>(IEnumerable<T> entities) =>
            AddToDb(entities.OfType<object>().ToArray());

        protected async Task AddToDb(params object[] entities)
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            dbContext.AddRange(entities);

            await dbContext.SaveChangesAsync();
        }

        protected async Task AddToDb<T>(T entity)
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            dbContext.Add(entity);

            await dbContext.SaveChangesAsync();
        }

        protected async Task<T?> GetFromDb<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            return await dbContext.Set<T>()
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        private async Task EnsureDatabaseAsync()
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
