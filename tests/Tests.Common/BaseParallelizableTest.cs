using System.Threading.Tasks;
using NUnit.Framework;

namespace YoutubeMusicBot.IntegrationTests.Common
{
    [Parallelizable(ParallelScope.All)]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public abstract class BaseParallelizableTest
    {
        [SetUp]
        public virtual async ValueTask SetUp()
        {
        }

        [TearDown]
        public virtual async ValueTask TearDown()
        {
        }
    }
}
