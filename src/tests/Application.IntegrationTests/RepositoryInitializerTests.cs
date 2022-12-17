using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.IntegrationTests.Core;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.IntegrationTests;

[Parallelizable(ParallelScope.None)]
public class RepositoryInitializerTests : BaseIntegrationTest
{
    [Test]
    // test must be called first because of static id sequence indexer in AggregateBase
    [Order(int.MinValue)]
    public async Task ShouldInitializeWithoutDataInDb()
    {
        var sut = Container.Create<RepositoryInitializer<Message>>();

        await sut.StartAsync();

        var message = FixtureInstance.Create<Message>();
        message.Should().NotBeNull();
    }

    [Test]
    [CustomAutoData]
    public async Task ShouldInitializeWithDataInDb(long aggregateId)
    {
        var events = FixtureInstance.Build<MessageCreatedEvent>()
            .With(c => c.AggregateId, aggregateId)
            .CreateMany()
            .ToArray();
        await AddToDb(events);
        var sut = Container.Create<RepositoryInitializer<Message>>();

        await sut.StartAsync();

        var message = FixtureInstance.Create<Message>();
        message.Id.Should().Be(aggregateId + 1);
        message.GetUncommittedEvents().First().Id.Should().Be(events.Max(e => e.Id) + 1);
    }
}
