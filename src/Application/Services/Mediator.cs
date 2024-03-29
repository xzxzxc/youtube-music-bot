﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Services
{
    public class Mediator : IMediator
    {
        private readonly ILifetimeScope _scope;
        private readonly ConcurrentDictionary<string, Action> _cancellationActionsCache = new();

        public Mediator(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async ValueTask Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : ICommand
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<ICommandHandler<TRequest>>();
            await handler.Handle(request, cancellationToken);
        }

        public async ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : ICommand<TResult>
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<IRequestHandler<TRequest, TResult>>();
            return await handler.Handle(request, cancellationToken);
        }

        public async ValueTask Emit<TAggregate>(
            EventBase<TAggregate> @event,
            CancellationToken cancellationToken = default)
            where TAggregate : AggregateBase<TAggregate>
        {
            using var cancellationHolder = new CancellationHolder<TAggregate>(
                @event,
                cancellationToken,
                _cancellationActionsCache);
            var handlerType = typeof(IEventHandler<,>).MakeGenericType(
                @event.GetType(),
                typeof(TAggregate));
            await using var scope = _scope.BeginLifetimeScope();
            dynamic? handler = scope.ResolveOptional(handlerType);
            if (handler != null)
                await handler.Handle((dynamic)@event, cancellationHolder.CancellationToken);
        }

        public void Cancel<TAggregate>(long aggregateId)
            where TAggregate : AggregateBase<TAggregate>
        {
            var cancellationAction = CancellationHolder<TAggregate>.GetCancelAction(
                aggregateId,
                _cancellationActionsCache);
            cancellationAction();
        }

        private class CancellationHolder<TAggregate> : IDisposable
            where TAggregate : AggregateBase<TAggregate>
        {
            private readonly string _cacheKey;
            private readonly ConcurrentDictionary<string, Action> _cancellationActions;
            private readonly CancellationTokenSource _aggregateSource;
            private readonly bool _shouldRemoveOnDispose;

            public CancellationHolder(
                EventBase<TAggregate> @event,
                CancellationToken initialToken,
                ConcurrentDictionary<string, Action> cancellationActions)
            {
                _cancellationActions = cancellationActions;
                _cacheKey = CreateCacheKey(@event.AggregateId);
                _aggregateSource = CancellationTokenSource.CreateLinkedTokenSource(initialToken);
                _shouldRemoveOnDispose = _cancellationActions.TryAdd(_cacheKey, Cancel);
            }

            private void Cancel() =>
                _aggregateSource.Cancel();

            public CancellationToken CancellationToken => _aggregateSource.Token;

            public void Dispose()
            {
                if (_shouldRemoveOnDispose)
                    _cancellationActions.Remove(_cacheKey, out _);
            }

            public static Action GetCancelAction(
                long aggregateId,
                ConcurrentDictionary<string, Action> cancellationActionsCache)
            {
                var cacheKey = CreateCacheKey(aggregateId);
                return cancellationActionsCache[cacheKey];
            }

            private static string CreateCacheKey(long aggregateId) =>
                $"{typeof(TAggregate).Name}{aggregateId}";
        }
    }
}
