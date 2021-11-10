using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.EventHandlers;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.EventHandlers
{
    public class MessageFinishedHandlerTests : BaseParallelizableTest
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
