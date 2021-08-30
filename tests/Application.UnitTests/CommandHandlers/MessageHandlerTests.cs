using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.CommandHandlers;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.CommandHandlers
{
    [Parallelizable]
    public class MessageHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCreateMessageAggregate(
            MessageHandler.Command command)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageHandler>();

            await sut.Handle(command);

            Message? message = null;
            var match = new CaptureMatch<Message>(m => message = m);
            container.Mock<IRepository<Message>>().Verify(
                r => r.SaveAndEmitEventsAsync(Capture.With(match), It.IsAny<CancellationToken>()),
                Times.Once);
            message.Should().NotBeNull();
            message!.ExternalId.Should().Be(command.MessageId);
        }
    }
}
