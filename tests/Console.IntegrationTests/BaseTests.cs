using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeMusicBot.Tests.Common;
using static Console.IntegrationTest.CommonFixture;

namespace Console.IntegrationTest
{
    public class BaseTests
    {
        [SetUp]
        public virtual async ValueTask SetUp()
        {
            ThrowExceptionLogger.Errors.Clear();

            if (GerCacheFolder.Exists)
                GerCacheFolder.Delete(recursive: true);

            GerCacheFolder.Create();
        }

        [TearDown]
        public virtual async ValueTask TearDown()
        {
            if (GerCacheFolder.Exists)
                GerCacheFolder.Delete(recursive: true);
        }
    }
}
