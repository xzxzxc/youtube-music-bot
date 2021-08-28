using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests.Services
{
    public class MediatorServiceTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCallRequestHandler(SimpleTestRequest request)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();

            await sut.Send(request);

            SimpleTestRequestHandler.Called.Should().BeTrue();
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
        public async Task ShouldCallEventHandler(SimpleTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();

            await sut.Emit(@event);

            SimpleTestEventHandler.Called.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScopeOnRequest(SimpleTestRequest request)
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
        public async Task ShouldCreateLifetimeScopeOnRequestWithResponse(
            TestRequestWithResponse request)
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
        public async Task ShouldCreateLifetimeScopeOnEvent(SimpleTestEvent @event)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<MediatorService>();

            await sut.Emit(@event);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelRequestHandling(SimpleTestRequest request)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();
            var source = new CancellationTokenSource();
            var sendTask = new Func<Task>(() => sut.Send(request, source.Token).AsTask());

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelRequestWithResponseHandling(TestRequestWithResponse request)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();
            var source = new CancellationTokenSource();
            var sendTask = new Func<Task<Guid>>(
                () => sut.Send<TestRequestWithResponse, Guid>(request, source.Token)
                    .AsTask());

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelEventHandlingUsingCancellationToken(InfiniteHandleTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<MediatorService>();
            var source = new CancellationTokenSource();
            var sendTask = new Func<Task>(() => sut.Emit(@event, source.Token).AsTask());

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelEventHandlingUsingCancellationId(
            InfiniteHandleTestEvent @event)
        {
            // TODO:
            // var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            // using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            // var sut = container.Create<MediatorService>();
            // var emitTask = sut.Emit(@event).AsTask();
            //
            // sut.Cancel(@event.GetCancellationId());
            //
            // Func<Task> getEmitTask = () => emitTask;
            // Func<Task> getEmitWithinTask =
            //     () => getEmitTask.Should().CompleteWithinAsync(1.Seconds());
            // await getEmitWithinTask.Should().ThrowAsync<OperationCanceledException>();
        }

        public record SimpleTestRequest : IRequest;

        public class SimpleTestRequestHandler : IRequestHandler<SimpleTestRequest>
        {
            public static bool Called = false;

            public async ValueTask Handle(
                SimpleTestRequest request,
                CancellationToken cancellationToken)
            {
                Called = true;
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public record TestRequestWithResponse : IRequest<Guid>;

        public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, Guid>
        {
            public static Guid Response = Guid.NewGuid();

            public async ValueTask<Guid> Handle(
                TestRequestWithResponse request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Response;
            }
        }

        public class TestAggregate : AggregateBase<TestAggregate>
        {
        }

        public record SimpleTestEvent : EventBase<TestAggregate>;

        public class SimpleTestEventHandler : IEventHandler<SimpleTestEvent, TestAggregate>
        {
            public static bool Called = false;

            public async ValueTask Handle(
                SimpleTestEvent @event,
                CancellationToken cancellationToken)
            {
                Called = true;
            }
        }

        public record InfiniteHandleTestEvent : EventBase<TestAggregate>;

        public class InfiniteHandleTestEventHandler :
            IEventHandler<InfiniteHandleTestEvent, TestAggregate>
        {
            public async ValueTask Handle(
                InfiniteHandleTestEvent @event,
                CancellationToken cancellationToken)
            {
                await Task.Delay(-1, cancellationToken); // -1 is infinite
            }
        }
    }
}
