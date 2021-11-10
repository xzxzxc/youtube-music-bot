using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.CommandHandlers;
using YoutubeMusicBot.Application.UnitTests.Extensions;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.CommandHandlers
{
    public class CancelMessageHandlerTests : BaseParallelizableTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCancel(CancelMessageHandler.Command command)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CancelMessageHandler>();

            await sut.Handle(command);

            container.Mock<IMediator>()
                .Verify(m => m.Cancel<Message>(command.MessageId), Times.Once);
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

            await sut.Handle(new CancelMessageHandler.Command(message.Id));

            var uncommittedEvents = message.GetUncommittedEvents();
            uncommittedEvents.Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageFinishedEvent>();
            container.VerifyMessageSaved(message);
        }
    }
}
