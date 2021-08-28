using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests.Services
{
    public class CallbackDataFactoryTests
    {
        [Test]
        [CustomAutoData]
        public void ShouldCreateForCancel(
            MessageCreatedEvent @event)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CallbackDataFactory>();

            var res = sut.CreateForCancel(@event);

            res.Should().Be($"c{@event.AggregateId}");
        }

        [Test]
        [CustomAutoData]
        public void ShouldParseCancel(
            string eventCancellationId)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<CallbackDataFactory>();

            var res = sut.Parse($"c{eventCancellationId}");

            res.Should().BeOfType<CancelResult>()
                .Which.EventCancellationId.Should().Be(eventCancellationId);
        }
    }
}
