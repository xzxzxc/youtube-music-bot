using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    [Parallelizable]
    public class NewTrackHandlerTests
    {
        [Test]
        [CustomInlineAutoData(true, false)]
        [CustomInlineAutoData(false, true)]
        public async Task ShouldTrySplitFileIfNotSkipped(
            bool skipSplit,
            bool shouldSendSplitRequest,
            MessageContext messageContext)
        {
            var file = Mock.Of<IFileInfo>(f => f.Exists);
            var mediatorMock = new Mock<IMediator>();
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterMock(mediatorMock);
                    b.RegisterInstance(messageContext);
                });
            var handler = container.Create<NewTrackHandler>();

            await handler.Handle(new NewTrackHandler.Request(file, skipSplit));

            mediatorMock.Verify(
                m => m.Send<TrySplitHandler.Request, bool>(
                    It.Is<TrySplitHandler.Request>(
                        r => r.File == file),
                    It.IsAny<CancellationToken>()),
                shouldSendSplitRequest
                    ? Times.Once
                    : Times.Never);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldSendFileIfNotSplit(MessageContext messageContext)
        {
            var file = Mock.Of<IFileInfo>(f => f.Exists);
            var tgClientMock = new Mock<ITgClientWrapper>();
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(
                    m => m.Send<TrySplitHandler.Request, bool>(
                        It.Is<TrySplitHandler.Request>(
                            r => r.File == file),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterMock(tgClientMock);
                    b.RegisterMock(mediatorMock);
                    b.RegisterInstance(messageContext);
                });
            var handler = container.Create<NewTrackHandler>();

           var res = await handler.Handle(new NewTrackHandler.Request(file));

            tgClientMock.Verify(c => c.SendAudioAsync(file, It.IsAny<CancellationToken>()));
            res.Should().Be(true);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldDeleteTrackFileOnDispose(MessageContext messageContext)
        {
            // TODO: create folder for each request and remove folder on message aggregate end, then remove this shit
            var fileMock = new Mock<IFileInfo>();
            fileMock.Setup(f => f.Exists).Returns(true);
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterInstance(messageContext));
            var sut = container.Create<NewTrackHandler>();
            await sut.Handle(new NewTrackHandler.Request(fileMock.Object));

            sut.Dispose();

            fileMock.Verify(f => f.Delete(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldDeleteDescriptionFileOnDispose(MessageContext messageContext)
        {
            // TODO: create folder fore each request and remove folder on message aggregate end, then remove this shit
            var descriptionFileMock = new Mock<IFileInfo>();
            descriptionFileMock.Setup(f => f.Exists).Returns(true);
            var file = Mock.Of<IFileInfo>(f => f.Exists);
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterInstance(messageContext);
                    b.RegisterInstance(
                        Mock.Of<IDescriptionService>(
                            s => s.GetFileOrNull(file) == descriptionFileMock.Object));
                });
            var sut = container.Create<NewTrackHandler>();
            await sut.Handle(new NewTrackHandler.Request(file));

            sut.Dispose();

            descriptionFileMock.Verify(f => f.Delete(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldForceSplitIfFileIsTooLarge(
            MessageContext messageContext,
            int fileLength)
        {
            var file = Mock.Of<IFileInfo>(f => f.Exists && f.Length == fileLength);
            var mediatorMock = new Mock<IMediator>();
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterOptions(new BotOptions { FileBytesLimit = fileLength - 1, });
                    b.RegisterMock(mediatorMock);
                    b.RegisterInstance(messageContext);
                });
            var sut = container.Create<NewTrackHandler>();

            await sut.Handle(new NewTrackHandler.Request(file, SkipSplit: false));

            mediatorMock.Verify(
                m => m.Send<TrySplitHandler.Request, bool>(
                    It.Is<TrySplitHandler.Request>(
                        r => r.File == file
                         && r.FileIsTooLarge == true),
                    It.IsAny<CancellationToken>()));
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldReturnFalseIfSkipSplitAndFileIsTooLarge(
            MessageContext messageContext,
            int fileLength)
        {
            var file = Mock.Of<IFileInfo>(f => f.Exists && f.Length == fileLength);
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterOptions(new BotOptions { FileBytesLimit = fileLength - 1, });
                    b.RegisterInstance(messageContext);
                });
            var sut = container.Create<NewTrackHandler>();

            var res = await sut.Handle(new NewTrackHandler.Request(file, SkipSplit: true));

            res.Should().BeFalse();
        }
    }
}
