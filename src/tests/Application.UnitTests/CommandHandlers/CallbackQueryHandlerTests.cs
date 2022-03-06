using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.CommandHandlers;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using YoutubeMusicBot.IntegrationTests.Common.Moq;

namespace YoutubeMusicBot.Application.UnitTests.CommandHandlers
{
    public class CallbackQueryHandlerTests : BaseParallelizableTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCallCancelMessageHandler(
            CallbackQueryHandler.Command command,
            CancelResult<Message> cancelResult)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CallbackQueryHandler>();
            container.Mock<ICallbackDataFactory>()
                .Setup(c => c.Parse(command.CallbackData!))
                .Returns(cancelResult);

            await sut.Handle(command);

            var lazyCapture = new LazyCapture<CancelMessageHandler.Command>();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(
                        Capture.With(lazyCapture.Match),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            lazyCapture.Value.MessageId.Should().Be(cancelResult.AggregateId);
        }
    }
}
