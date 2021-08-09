using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Console.Handlers;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;
using YoutubeMusicBot.Console.Wrappers;
using YoutubeMusicBot.Console.Wrappers.Interfaces;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    [Parallelizable]
    public class YoutubeDlWrapperTests
    {
        private readonly IFixture _fixture;

        public YoutubeDlWrapperTests()
        {
            _fixture = AutoFixtureFactory.Create();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRunCorrectShellCommand(
            string cacheFolder,
            string configFilePath,
            string url)
        {
            ProcessRunner.Request? request = null;
            var processRunnerMock = new Mock<IProcessRunner>();
            processRunnerMock.Setup(
                    r => r.RunAsync(
                        It.IsAny<ProcessRunner.Request>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ProcessRunner.Request, CancellationToken>((r, _) => request = r)
                .Returns(AsyncEnumerable.Empty<ProcessRunner.Line>())
                .Verifiable();
            using var container = CreateAutoMockContainer(
                b =>
                {
                    b.RegisterMock(processRunnerMock);
                    b.RegisterInstance(Mock.Of<ICacheFolder>(f => f.Value == cacheFolder));
                    b.RegisterInstance(
                        Mock.Of<IYoutubeDlConfigPath>(
                            r => r.GetValueAsync(It.IsAny<CancellationToken>()).Result == configFilePath));
                });
            var wrapper = container.Create<YoutubeDlWrapper>();

            await wrapper.DownloadAsync(url).ToListAsync();

            request.Should().NotBeNull(
                $"{nameof(IProcessRunner.RunAsync)} wasn't called."
                + $" Preformed invocations {processRunnerMock.Invocations}.");
            request!.ProcessName.Should().Be("youtube-dl");
            request.WorkingDirectory.Should().Be(cacheFolder);
            request.Arguments.Should()
                .SatisfyRespectively(
                    a1 => a1.Should().Be("--config-location"),
                    a2 => a2.Should().Be(configFilePath),
                    a3 => a3.Should().Be(url));
        }

        [Test]
        [CustomInlineAutoData(
            "Test title",
            "Loading \"Test title\" started.",
            @"
test line
[info] Writing video description to: NA-Test title.description
test line")]
        public async Task ShouldUpdateReplyMessageWithTitle(
            string title,
            string messageText,
            string youtubeDlOutput,
            MessageContext messageContext)
        {
            // TODO: add integration test for real output
            using var container = CreateAutoMockContainer(
                b => b.RegisterInstance(CreateMockedProcessRunner(youtubeDlOutput)),
                messageContext);
            var wrapper = container.Create<YoutubeDlWrapper>();

            await wrapper.DownloadAsync(_fixture.Create<string>()).ToListAsync();

            container.Mock<ITgClientWrapper>()
                .Verify(
                    w => w.UpdateMessageAsync(
                        messageContext.MessageToUpdate!.Id,
                        messageText,
                        messageContext.MessageToUpdate.InlineButton,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        [CustomInlineAutoData(
            "test.mp3",
            @"
test line
[info] Writing video description to: NA-Test title.description
[completed] test.mp3
test line")]
        public async Task ShouldReturnFile(
            string fileName,
            string youtubeDlOutput,
            string url,
            string cacheFolder)
        {
            var mediatorMock = new Mock<IMediator>();
            using var container = CreateAutoMockContainer(
                b =>
                {
                    b.RegisterInstance(Mock.Of<ICacheFolder>(f => f.Value == cacheFolder));
                    b.RegisterInstance(CreateMockedProcessRunner(youtubeDlOutput));
                    b.RegisterMock(mediatorMock);
                });
            var wrapper = container.Create<YoutubeDlWrapper>();

            var file = await wrapper.DownloadAsync(_fixture.Create<string>()).SingleAsync();

            file.Should().NotBeNull();
            file.Name.Should().Be(fileName);
            file.DirectoryName.Should().NotBeNullOrEmpty();
            file.DirectoryName.Should().EndWith(cacheFolder);
        }

        private IProcessRunner CreateMockedProcessRunner(string lines)
        {
            var linesEnumerable = lines.Split("\n")
                .Select(l => new ProcessRunner.Line(l))
                .ToAsyncEnumerable();
            return Mock.Of<IProcessRunner>(
                s => s.RunAsync(
                        It.Is<ProcessRunner.Request>(r => r.ProcessName == "youtube-dl"),
                        It.IsAny<CancellationToken>())
                    == linesEnumerable);
        }

        private AutoMock CreateAutoMockContainer(
            Action<ContainerBuilder>? beforeBuild = null,
            MessageContext? messageContext = null) =>
            AutoMockContainerFactory.Create(
                builder =>
                {
                    builder.RegisterInstance(
                        messageContext
                        ?? _fixture.Create<MessageContext>());
                    beforeBuild?.Invoke(builder);
                });
    }
}
