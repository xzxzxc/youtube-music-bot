using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;
using static YoutubeMusicBot.AcceptanceTest.CommonFixture;

namespace YoutubeMusicBot.AcceptanceTest
{
    public class BaseTests
    {
        [SetUp]
        public virtual async ValueTask SetUp()
        {
            ThrowExceptionLogger.Errors.Clear();

            await TempFolder.WaitToDelete(recursive: true);

            TempFolder.Create();
        }

        [TearDown]
        public virtual async ValueTask TearDown()
        {
            await TempFolder.WaitToDelete(recursive: true);
        }
    }
}
