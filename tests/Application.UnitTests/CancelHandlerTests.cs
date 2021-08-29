using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.UnitTests.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    public class CancelHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCancel(CancelMessageHandler.Request request)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CancelMessageHandler>();

            await sut.Handle(request);

            container.Mock<IMediator>()
                .Verify(m => m.Cancel<Message>(request.MessageId), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMessageFinishedEvent(Message message)
        {
            message.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<IRepository<Message>>()
                .Setup(m => m.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(message);
            var sut = container.Create<CancelMessageHandler>();

            await sut.Handle(new CancelMessageHandler.Request(message.Id));

            var uncommittedEvents = message.GetUncommittedEvents();
            uncommittedEvents.Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageFinishedEvent>();
            container.VerifyMessageSaved(message);
        }
    }
}
