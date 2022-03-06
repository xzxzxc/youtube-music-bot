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
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Application.UnitTests.Services
{
    public class MediatorServiceTests : BaseParallelizableTest
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldCallRequestHandler(SimpleTestCommand command)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();

            await sut.Send(command);

            SimpleTestCommandHandler.Called.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldReturnValueFromHandler(TestCommandWithResponse command)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();

            var res = await sut.Send<TestCommandWithResponse, Guid>(command);

            res.Should().Be(TestRequestWithResponseHandler.Response);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCallEventHandler(SimpleTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();

            await sut.Emit(@event);

            SimpleTestEventHandler.Called.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScopeOnRequest(SimpleTestCommand command)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<Mediator>();

            await sut.Send(command);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCreateLifetimeScopeOnRequestWithResponse(
            TestCommandWithResponse command)
        {
            var scopeMock = new Mock<ILifetimeScope>();
            using var container = AutoMockContainerFactory.Create(b => b.RegisterMock(scopeMock));
            scopeMock.Setup(m => m.BeginLifetimeScope())
                .Returns(AutoMockContainerFactory.Create().Create<ILifetimeScope>());
            var sut = container.Create<Mediator>();

            var res = await sut.Send<TestCommandWithResponse, Guid>(command);

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
            var sut = container.Create<Mediator>();

            await sut.Emit(@event);

            scopeMock.Verify(s => s.BeginLifetimeScope(), Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelRequestHandling(InfiniteHandleTestCommand command)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();
            var source = new CancellationTokenSource();
            var task = Task.Run(() => sut.Send(command, source.Token).AsTask());
            var sendTask = new Func<Task>(() => task);

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelRequestWithResponseHandling(
            InfiniteHandleTestCommandWithResponse command)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();
            var source = new CancellationTokenSource();
            var task = Task.Run(
                () => sut.Send<InfiniteHandleTestCommandWithResponse, object?>(
                        command,
                        source.Token)
                    .AsTask());
            var sendTask = new Func<Task>(() => task);

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelEventHandlingUsingCancellationToken(
            InfiniteHandleTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();
            var source = new CancellationTokenSource();
            var sendTask = new Func<Task>(() => sut.Emit(@event, source.Token).AsTask());

            source.Cancel();

            await sendTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelEventHandlingUsingAggregateId(InfiniteHandleTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Create<Mediator>();
            var emitTask = sut.Emit(@event).AsTask();

            sut.Cancel<TestAggregate>(@event.AggregateId);

            Func<Task> getEmitTask = () => emitTask;
            Func<Task> getEmitWithinTask =
                () => getEmitTask.Should().CompleteWithinAsync(1.Seconds());
            await getEmitWithinTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCancelNestedEventHandling(NestedEventTestEvent @event)
        {
            var module = new MediatorModule(assembliesToScan: Assembly.GetExecutingAssembly());
            using var container = AutoMockContainerFactory.Create(b => b.RegisterModule(module));
            var sut = container.Container.Resolve<IMediator>();
            var emitTask = sut.Emit(@event, default).AsTask();

            sut.Cancel<TestAggregate>(@event.AggregateId);

            Func<Task> getEmitTask = () => emitTask;
            Func<Task> getEmitWithinTask =
                () => getEmitTask.Should().CompleteWithinAsync(1.Seconds());
            await getEmitWithinTask.Should().ThrowAsync<OperationCanceledException>();
        }

        public record SimpleTestCommand : ICommand;

        public class SimpleTestCommandHandler : ICommandHandler<SimpleTestCommand>
        {
            public static bool Called = false;

            public async ValueTask Handle(
                SimpleTestCommand command,
                CancellationToken cancellationToken)
            {
                Called = true;
            }
        }

        public record TestCommandWithResponse : ICommand<Guid>;

        public class TestRequestWithResponseHandler : IRequestHandler<TestCommandWithResponse, Guid>
        {
            public static Guid Response = Guid.NewGuid();

            public async ValueTask<Guid> Handle(
                TestCommandWithResponse command,
                CancellationToken cancellationToken)
            {
                return Response;
            }
        }

        public record InfiniteHandleTestCommand : ICommand;

        public class InfiniteHandleTestCommandHandler :
            ICommandHandler<InfiniteHandleTestCommand>
        {
            public ValueTask Handle(
                InfiniteHandleTestCommand command,
                CancellationToken cancellationToken) =>
                new(Task.Delay(Timeout.Infinite, cancellationToken));
        }

        public record InfiniteHandleTestCommandWithResponse : ICommand<object?>;

        public class InfiniteHandleTestRequestWithResponseHandler :
            IRequestHandler<InfiniteHandleTestCommandWithResponse, object?>
        {
            public async ValueTask<object?> Handle(
                InfiniteHandleTestCommandWithResponse command,
                CancellationToken cancellationToken)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return null;
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
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
        }

        public record NestedEventTestEvent : EventBase<TestAggregate>;

        public class NestedEventTestEventHandler :
            IEventHandler<NestedEventTestEvent, TestAggregate>
        {
            private readonly IMediator _mediator;

            public NestedEventTestEventHandler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async ValueTask Handle(
                NestedEventTestEvent @event,
                CancellationToken cancellationToken)
            {
                await _mediator.Emit(
                    new InfiniteHandleTestEvent
                    {
                        AggregateId = @event.AggregateId, Aggregate = @event.Aggregate
                    },
                    cancellationToken);
            }
        }
    }
}
