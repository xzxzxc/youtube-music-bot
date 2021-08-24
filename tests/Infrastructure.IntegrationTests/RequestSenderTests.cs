using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Tests.Common;

namespace Infrastructure.IntegrationTests
{
    public class MediatorServiceTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCallRequestHandler(TestRequest request)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();

            await sut.Send(request);

            TestRequestHandler.Called.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldReturnValueFromHandler(TestRequestWithResponse request)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();

            var res = await sut.Send<TestRequestWithResponse, Guid>(request);

            res.Should().Be(TestRequestWithResponseHandler.Response);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScope(TestRequest request)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<MediatorService>();

            await sut.Send(request);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScope(TestRequestWithResponse request)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<MediatorService>();

            var res = await sut.Send<TestRequestWithResponse, Guid>(request);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }
        [Test]
        [CustomAutoData]
        public async Task ShouldCallEventHandler(TestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();

            await sut.Emit(@event);

            TestEventHandler.Called.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScope(TestEvent @event)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<MediatorService>();

            await sut.Emit(@event);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }

        public record TestRequest : IRequest;

        public class TestRequestHandler : IRequestHandler<TestRequest>
        {
            public static bool Called = false;

            public async ValueTask Handle(TestRequest request, CancellationToken cancellationToken)
            {
                Called = true;
            }
        }

        public record TestRequestWithResponse : IRequest<Guid>;

        public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, Guid>
        {
            public static Guid Response = Guid.NewGuid();

            public async ValueTask<Guid> Handle(
                TestRequestWithResponse request,
                CancellationToken cancellationToken) =>
                Response;
        }

        public class TestAggregate : AggregateBase<TestAggregate>
        {
        }

        public record TestEvent : EventBase<TestAggregate>;

        public class TestEventHandler : IEventHandler<TestEvent, TestAggregate>
        {
            public static bool Called = false;

            public async ValueTask Handle(TestEvent @event, CancellationToken cancellationToken)
            {
                Called = true;
            }
        }
    }
}
