using System.Threading.Tasks;
using Autofac;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using static YoutubeMusicBot.AcceptanceTest.CommonFixture;

namespace YoutubeMusicBot.AcceptanceTest
{
    public class HostTests : BaseTests
    {
        [Test]
        [Timeout(2_000)] // 2 sec
        public async Task ShouldGracefullyShutDown()
        {
            // TODO: implement for testcontainers
            // var hostLifetime = RootScope.Resolve<IHostApplicationLifetime>();
            //
            // await Task.Delay(1.Seconds());
            // hostLifetime.StopApplication();
            //
            // await HostRunTask;
            //
            // CheckNoErrorsLogged();
        }
    }
}
