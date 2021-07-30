using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using MediatR;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.UnitTests
{
    public class NewTrackHandlerTests
    {
        [Test]
        [CustomInlineAutoData(true)]
        [CustomInlineAutoData(false)]
        public async Task ShouldTrySplitFileIfRequested(
            bool trySplit,
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

            await handler.Handle(new NewTrackHandler.Request(file, trySplit));

            mediatorMock.Verify(
                m => m.Send(
                    It.Is<TrySplitHandler.Request>(
                        r => r.File == file),
                    It.IsAny<CancellationToken>()),
                trySplit
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
                    m => m.Send(
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

            await handler.Handle(new NewTrackHandler.Request(file));

            tgClientMock.Verify(c => c.SendAudioAsync(file, It.IsAny<CancellationToken>()));
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldDeleteTrackFileOnDispose(MessageContext messageContext)
        {
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
    }
}
