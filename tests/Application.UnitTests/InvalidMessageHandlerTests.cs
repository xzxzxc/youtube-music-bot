using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class InvalidMessageHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendValidationMessageToUser(MessageInvalidEvent @event)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<InvalidMessageHandler>();

            await sut.Handle(@event);

            container.Mock<ITgClient>()
                .Verify(
                    c => c.SendMessageAsync(
                        @event.Aggregate.ChatId,
                        @event.ValidationMessage,
                        null,
                        It.IsAny<CancellationToken>()));
        }
    }
}
