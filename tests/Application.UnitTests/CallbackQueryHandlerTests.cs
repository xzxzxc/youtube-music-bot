using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class CallbackQueryHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCallCancelMessageHandler(
            CallbackQueryHandler.Request request,
            CancelResult<Message> cancelResult)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CallbackQueryHandler>();
            container.Mock<ICallbackDataFactory>()
                .Setup(c => c.Parse(request.Value.CallbackData!))
                .Returns(cancelResult);

            await sut.Handle(request);

            var lazyCapture = new LazyCapture<CancelMessageHandler.Request>();
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
