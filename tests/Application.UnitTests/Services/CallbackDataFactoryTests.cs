using System.Reflection;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests.Services
{
    public class CallbackDataFactoryTests
    {
        [Test]
        [CustomAutoData]
        public void ShouldParseCreatedForCancel(SimpleTestEvent @event)
        {
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterModule(
                    new CallbackDataModule(Assembly.GetExecutingAssembly())));
            var sut = container.Container.Resolve<ICallbackDataFactory>();
            var callbackData = sut.CreateForCancel(@event);

            var result = sut.Parse(callbackData);

            result.Should()
                .BeOfType<CancelResult<TestAggregate>>()
                .Which.AggregateId.Should()
                .Be(@event.AggregateId);
        }

        public class TestAggregate : AggregateBase<TestAggregate>
        {
        }

        public record SimpleTestEvent : EventBase<TestAggregate>;
    }
}
