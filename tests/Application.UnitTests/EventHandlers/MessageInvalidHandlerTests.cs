using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.EventHandlers;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.EventHandlers
{
    [Parallelizable]
    public class MessageInvalidHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendValidationMessageToUser(MessageInvalidEvent @event)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageInvalidHandler>();

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
