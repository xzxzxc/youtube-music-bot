using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Tests.Common;

namespace Infrastructure.IntegrationTests
{
    [Parallelizable]
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

        [Test(Description = "You need to kill process if test fails")]
        [Timeout(5_000)]
        [InlineAutoData(0)]
        [InlineAutoData(5)]
        [InlineAutoData(50)]
        [InlineAutoData(500)]
        public async Task ShouldCancel(
            int millisecondsToWait)
        {
            var tokenSource = new CancellationTokenSource();
            using var container = AutoMockContainerFactory.Create();
            var runProcessHandler = container.Create<ProcessRunner>();
            // task run to force execution on another thread
            var task = Task.Run(() => runProcessHandler.RunAsync(
                    new ProcessRunner.Request(
                        "sleep",
                        WorkingDirectory: ".",
                        Arguments: "infinity"),
                    tokenSource.Token)
                .GetAsyncEnumerator()
                .MoveNextAsync()
                .AsTask()
            );
            Func<Task> getTask = () => task;
            // add time for call
            await Task.Delay(millisecondsToWait.Milliseconds());

            tokenSource.Cancel();

            await getTask.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
