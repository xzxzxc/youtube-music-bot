using System.Reflection;
using System.Text;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.Services
{
    public class CallbackDataFactoryTests : BaseParallelizableTest
    {
        private const int TelegramMaxCallbackDataSize = 64;

        [Test]
        [CustomAutoData]
        public void ShouldCreateForCancelNotTooBigString(SimpleTestEvent @event)
        {
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterModule(
                    new CallbackDataModule(Assembly.GetExecutingAssembly())));
            var sut = container.Container.Resolve<ICallbackDataFactory>();

            var callbackData = sut.CreateForCancel(@event);

            Encoding.ASCII.GetBytes(callbackData)
                .Should()
                .HaveCountLessOrEqualTo(TelegramMaxCallbackDataSize);
        }

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
