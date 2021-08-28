using System.IO;
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
    public class FileToBeSentCreatedHandlerTests
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
