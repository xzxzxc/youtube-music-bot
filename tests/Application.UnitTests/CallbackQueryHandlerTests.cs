using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class CallbackQueryHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCancel(
            CallbackQueryHandler.Request request,
            string eventCancellationId)
        {
            using var container = AutoMockContainerFactory.Create();
            container.Mock<ICallbackDataFactory>()
                .Setup(c => c.Parse(request.Value.CallbackData!))
                .Returns(new CancelResult(eventCancellationId));
            var sut = container.Create<CallbackQueryHandler>();

            await sut.Handle(request);

            container.Mock<IMediator>().Verify(m => m.Cancel(eventCancellationId), Times.Once);
        }
    }
}
