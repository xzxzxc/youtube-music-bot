using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Tests.Common;

namespace Infrastructure.IntegrationTests
{
    public class ExceptionLogDecoratorTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCatchAndLogExceptionOnEmit(
            TestEvent @event,
            Exception exception)
        {
            var module = new MediatorModule(Assembly.GetExecutingAssembly());
            using var nestedContainer = AutoMockContainerFactory.Create(
                b => b.RegisterModule(module));
            // do not rewrite to AutoMock.Mock<>(). it wouldn't work https://github.com/autofac/Autofac.Extras.Moq/issues/43
            var mock = new Mock<IEventHandler<TestEvent, TestAggregate>>();
            mock.Setup(h => h.Handle(@event, It.IsAny<CancellationToken>()))
                .Throws(exception);
            using var container = AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterModule(module);
                    b.RegisterMock(mock);
                });
            var sut = container.Create<ExceptionLogDecorator>();

            Func<Task> action = () => sut.Emit(@event);

            (await action.Should().ThrowAsync<Exception>()).Which.InnerException.Should()
                .BeSameAs(exception);
            container.Mock<ILogger<ExceptionLogDecorator>>()
                .Verify(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        public class TestAggregate : AggregateBase<TestAggregate>
        {
        }

        public record TestEvent : EventBase<TestAggregate>;

        public record TestRequest : IRequest;

        public record TestRequestWithResponse : IRequest<object>;
    }
}
