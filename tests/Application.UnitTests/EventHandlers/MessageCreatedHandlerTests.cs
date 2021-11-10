using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.EventHandlers;
using YoutubeMusicBot.Application.UnitTests.Extensions;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.EventHandlers
{
    public class MessageCreatedHandlerTests : BaseParallelizableTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMessageValidOnValidUrl(MessageCreatedEvent @event)
        {
            @event.Aggregate.ClearUncommittedEvents();
            var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageCreatedHandler>();
            container.Mock<IValidator<Message>>()
                .Setup(v => v.ValidateAsync(@event.Aggregate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            await sut.Handle(@event);

            @event.Aggregate.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageValidEvent>();
            container.VerifyMessageSaved(@event.Aggregate);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMessageInvalidOnInvalidUrl(
            MessageCreatedEvent @event,
            string validationMessage)
        {
            @event.Aggregate.ClearUncommittedEvents();
            var container = AutoMockContainerFactory.Create();
            var sut = container.Create<MessageCreatedHandler>();
            container.Mock<IValidator<Message>>()
                .Setup(v => v.ValidateAsync(@event.Aggregate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ValidationResult(
                        new[]
                        {
                            new ValidationFailure(
                                nameof(@event.Aggregate.Text),
                                validationMessage)
                        }));

            await sut.Handle(@event);

            @event.Aggregate.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageInvalidEvent>()
                .Which.ValidationMessage.Should()
                .Be(validationMessage);
            container.VerifyMessageSaved(@event.Aggregate);
        }
    }
}
