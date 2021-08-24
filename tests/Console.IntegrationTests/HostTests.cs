using System.Threading.Tasks;
using Autofac;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using static Console.IntegrationTest.CommonFixture;

namespace Console.IntegrationTest
{
    public class HostTests : BaseTests
    {
        [Test]
        [Timeout(2_000)] // 2 sec
        public async Task ShouldGracefullyShutDown()
        {
            var hostLifetime = RootScope.Resolve<IHostApplicationLifetime>();

            await Task.Delay(1.Seconds());
            hostLifetime.StopApplication();

            await HostRunTask;

            CheckNoErrorsLogged();
        }
    }
}
