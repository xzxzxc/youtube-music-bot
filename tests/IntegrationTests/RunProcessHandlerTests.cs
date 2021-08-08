using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Console.Handlers;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.IntegrationTests
{
    public class ProcessRunnerTests
    {
        [Test]
        [AutoData]
        public async Task ShouldEcho(string message)
        {
            using var container = AutoMockContainerFactory.Create();
            var runProcessHandler = container.Create<ProcessRunner>();

            var outputLine = await runProcessHandler.RunAsync(
                    new ProcessRunner.Request(
                        "echo",
                        WorkingDirectory: ".",
                        Arguments: message))
                .FirstAsync();

            outputLine.Value.Should().Be(message);
        }
    }
}
