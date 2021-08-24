using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace Domain.UnitTests
{
    public class MessageTests
    {
        [Test]
        [CustomAutoData]
        public void ShouldRaiseMessageCreatedOnNew(Message message)
        {
            message.GetUncommittedEvents().Should().ContainSingle()
                .Which.Should().BeOfType<MessageCreatedEvent>()
                .Which.AggregateId.Should().Be(message.Id);
        }
    }
}
