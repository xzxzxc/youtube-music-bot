using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeMusicBot.IntegrationTests.Common;
using static YoutubeMusicBot.AcceptanceTest.CommonFixture;

namespace YoutubeMusicBot.AcceptanceTest
{
    public class BaseTests
    {
        [SetUp]
        public virtual async ValueTask SetUp()
        {
            ThrowExceptionLogger.Errors.Clear();

            if (TempaFolder.Exists)
                TempaFolder.Delete(recursive: true);

            TempaFolder.Create();
        }

        [TearDown]
        public virtual async ValueTask TearDown()
        {
            if (TempaFolder.Exists)
                TempaFolder.Delete(recursive: true);
        }
    }
}
