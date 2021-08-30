using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.EventHandlers;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Application.UnitTests.Extensions;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using YoutubeMusicBot.IntegrationTests.Common.Moq;

namespace YoutubeMusicBot.Application.UnitTests.EventHandlers
{
    [Parallelizable]
    public class MessageValidHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendLoadingStartedWithCancellation(
            MessageValidEvent @event,
            string callbackData,
            int processMessageId)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<ICallbackDataFactory>()
                .Setup(c => c.CreateForCancel(@event))
                .Returns(callbackData);
            var sut = container.Create<MessageValidHandler>();
            var lazyCapture = new LazyCapture<InlineButtonCollection>();
            container.Mock<ITgClient>()
                .Setup(
                    c => c.SendMessageAsync(
                        @event.Aggregate.ChatId,
                        "Loading started.",
                        Capture.With(lazyCapture.Match),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(processMessageId)
                .Verifiable();

            await sut.Handle(@event);

            lazyCapture.Value.VerifyCancelButton(callbackData);
            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            uncommittedEvents.Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<LoadingProcessMessageSentEvent>()
                .Which.MessageId.Should()
                .Be(processMessageId);
            container.VerifyMessageSaved(@event.Aggregate);
        }
    }
}
