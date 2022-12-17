using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.IntegrationTests.Core;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.IntegrationTests
{
    [Parallelizable(ParallelScope.None)]
    public class EventSourcingRepositoryTests : BaseIntegrationTest
    {
        [Test]
        public async Task ShouldPersistsAggregateState()
        {
            var message = FixtureInstance.Create<Message>();
            message.Valid();
            var sut = Container.Create<EventSourcingRepository<Message>>();

            await sut.SaveAndEmitEventsAsync(message);

            var messageFromRepo = await sut.GetByIdAsync(message.Id);
            messageFromRepo.Should().BeEquivalentTo(message);
        }
    }
}
