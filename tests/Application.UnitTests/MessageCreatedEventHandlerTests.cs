using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class MessageCreatedEventHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSetMessageValidOnValidUrl(MessageCreatedEvent @event)
        {
            @event.Aggregate.ClearUncommittedEvents();
            var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageCreatedEventHandler>();
            container.Mock<IValidator<Message>>()
                .Setup(v => v.ValidateAsync(@event.Aggregate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            await sut.Handle(@event);

            @event.Aggregate.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageValidEvent>();
            @event.Aggregate.IsValid.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldSetMessageInvalidOnInvalidUrl(
            MessageCreatedEvent @event,
            string validationMessage)
        {
            @event.Aggregate.ClearUncommittedEvents();
            var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageCreatedEventHandler>();
            container.Mock<IValidator<Message>>()
                .Setup(v => v.ValidateAsync(@event.Aggregate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ValidationResult(
                        new[] { new ValidationFailure(nameof(@event.Aggregate.Text), validationMessage) }));

            await sut.Handle(@event);

            @event.Aggregate.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageInvalidEvent>()
                .Which.ValidationMessage.Should()
                .Be(validationMessage);
            @event.Aggregate.IsValid.Should().BeFalse();
        }
    }
}
