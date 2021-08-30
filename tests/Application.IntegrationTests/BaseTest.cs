using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YoutubeMusicBot.Infrastructure.Database;
using static YoutubeMusicBot.Application.IntegrationTests.CommonFixture;

namespace YoutubeMusicBot.Application.IntegrationTests
{
    public abstract class BaseTest
    {
        [SetUp]
        public async Task SetUp()
        {
            if (DbFolder.Exists)
                DbFolder.Delete(recursive: true);
            await EnsureDatabaseAsync();
        }

        private static async Task EnsureDatabaseAsync()
        {
            await using var dbContext = Container.Create<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

    }
}
