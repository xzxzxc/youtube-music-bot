using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.IntegrationTests.Core;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.IntegrationTests
{
    public class ApplicationDbContextTests : BaseIntegrationTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldRestoreEventId(MessageCreatedEvent @event)
        {
            await AddToDb(@event);

            var eventFromDb = await GetFromDb<EventBase<Message>>(e => e.Id == @event.Id);

            eventFromDb.Should().NotBeNull();
            eventFromDb.Should()
                .BeEquivalentTo(@event, options => options.Excluding(e => e.Aggregate));
        }
    }
}
