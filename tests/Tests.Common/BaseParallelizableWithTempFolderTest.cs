using System.IO;
using System.Threading.Tasks;
using FluentAssertions.Extensions;

namespace YoutubeMusicBot.IntegrationTests.Common
{
    public abstract class BaseParallelizableWithTempFolderTest : BaseParallelizableTest
    {
        protected DirectoryInfo TempFolder { get; } = TempFolderFactory.Create();

        public override async ValueTask SetUp()
        {
            await base.SetUp();

            if (!TempFolder.Exists)
                TempFolder.Create();
        }

        public override async ValueTask TearDown()
        {
            await base.TearDown();

            if (TempFolder.Exists)
            {
                await Task.Delay(500.Milliseconds());
                TempFolder.Delete(recursive: true);
            }
        }
    }
}
