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
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;

namespace YoutubeMusicBot.Application.IntegrationTests
{
    [SetUpFixture]
    public static class CommonFixture
    {
        public static readonly DirectoryInfo DbFolder = new("temp_db");

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
                    builder.RegisterApplicationModules();
                    builder.RegisterModule(
                        new DbContextModule(DbFolder.FullName, enableSensitiveLogin: true));
                });
        }

        public static Task AddToDb<T>(IEnumerable<T> entities) =>
            AddToDb(entities.OfType<object>().ToArray());

        public static async Task AddToDb(params object[] entities)
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            dbContext.AddRange(entities);

            await dbContext.SaveChangesAsync();
        }

        public static async Task AddToDb<T>(T entity)
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            dbContext.Add(entity);

            await dbContext.SaveChangesAsync();
        }


        public static async Task<T?> GetFromDb<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            return await dbContext.Set<T>()
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            Container.Dispose();
        }
    }
}
