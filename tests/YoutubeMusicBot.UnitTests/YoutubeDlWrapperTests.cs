using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Wrappers;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.UnitTests
{
    public class YoutubeDlWrapperTests
    {
        private readonly IFixture _fixture;

        public YoutubeDlWrapperTests()
        {
            _fixture = AutoFixtureFactory.Create();
        }

        // TODO: add test for correct arguments

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
            var mediatorMock = new Mock<IMediator>();
            using var container = CreateAutoMockContainer(
                b =>
                {
                    b.RegisterMock(mediatorMock);
                    b.RegisterInstance(CreateMockedProcessRunner(youtubeDlOutput));
                },
                messageContext);
            var wrapper = container.Create<YoutubeDlWrapper>();

            await wrapper.DownloadAsync(_fixture.Create<string>());

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
        public async Task ShouldSendNewTrackRequest(
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

            await wrapper.DownloadAsync(_fixture.Create<string>());

            mediatorMock
                .Verify(
                    w => w.Send(
                        It.Is<NewTrackHandler.Request>(
                            r =>
                                r.File.Name == fileName
                                && r.File.DirectoryName != null
                                && r.File.DirectoryName.EndsWith(cacheFolder)
                                && r.TrySplit == true),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
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
