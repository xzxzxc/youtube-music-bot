using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class MessageFinishedHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldRemoveTempFolder(
            MessageFinishedEvent @event)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageFinishedHandler>();

            await sut.Handle(@event);

            container.Mock<IFileSystem>()
                .Verify(s => s.RemoveTempFolderAndContent(@event.AggregateId), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRemoveProcessMessage(
            MessageFinishedEvent @event,
            int processMessageId,
            string cacheFolder)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageFinishedHandler>();
            @event.Aggregate.LoadingProcessMessageSent(processMessageId);

            await sut.Handle(@event);

            container.Mock<ITgClient>()
                .Verify(
                    s => s.DeleteMessageAsync(
                        @event.Aggregate.ChatId,
                        processMessageId,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
