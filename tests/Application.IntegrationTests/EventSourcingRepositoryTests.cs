using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using static YoutubeMusicBot.Application.IntegrationTests.CommonFixture;

namespace YoutubeMusicBot.Application.IntegrationTests
{
    public class EventSourcingRepositoryTests : BaseTest
    {
        [Test]
        // test must be called first because of static id sequence indexer in AggregateBase
        [Order(int.MinValue)]
        public async Task ShouldInitializeWithoutDataInDb()
        {
            var sut = Container.Create<EventSourcingRepository<Message>>();

            await sut.Initialize();

            var message = FixtureInstance.Create<Message>();
            // + 2 because of one strange additional ctor call from AutoFixture
            message.Id.Should().BeInRange(long.MinValue + 1, long.MinValue + 2);
            message.GetUncommittedEvents()
                .First()
                .Id.Should()
                .BeInRange(long.MinValue + 1, long.MinValue + 2);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldInitializeWithDataInDb(long aggregateId)
        {
            var events = FixtureInstance.Build<MessageCreatedEvent>()
                .With(c => c.AggregateId, aggregateId)
                .CreateMany();
            await AddToDb(events);
            var sut = Container.Create<EventSourcingRepository<Message>>();

            await sut.Initialize();

            var message = FixtureInstance.Create<Message>();
            message.Id.Should().Be(aggregateId + 1);
            message.GetUncommittedEvents().First().Id.Should().Be(events.Max(e => e.Id) + 1);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldPersistsAggregateState(long aggregateId)
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
