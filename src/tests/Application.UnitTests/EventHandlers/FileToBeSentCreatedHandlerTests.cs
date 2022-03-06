using System.IO;
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
    public class FileToBeSentCreatedHandlerTests : BaseParallelizableTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendFileToUser(
            FileToBeSentCreatedEvent @event,
            Stream fileStream)
        {
            using var container = AutoMockContainerFactory.Create();
            container.Mock<IFileSystem>()
                .Setup(fs => fs.OpenReadStream(@event.FilePath))
                .Returns(fileStream);
            var sut = container.Create<FileToBeSentCreatedHandler>();

            await sut.Handle(@event);

            container.Mock<ITgClient>()
                .Verify(
                    c => c.SendAudioAsync(
                        @event.Aggregate.ChatId,
                        fileStream,
                        @event.Title,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
