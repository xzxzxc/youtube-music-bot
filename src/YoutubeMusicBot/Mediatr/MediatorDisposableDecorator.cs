using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Mediatr
{
	internal class MediatorDisposableDecorator : IMediator
	{
		private readonly ILifetimeScope _lifetimeScope;

		public MediatorDisposableDecorator(
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public async Task<TResponse> Send<TResponse>(
			IRequest<TResponse> request,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await using var scope = _lifetimeScope.BeginLifetimeScope();
				var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
				return await mediator.Send(request, cancellationToken);
			}
			catch (Exception ex) when (ex is not TaskCanceledException)
			{
				LogException(ex, request);
				throw;
			}
		}

		public async Task<object?> Send(
			object request,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await using var scope = _lifetimeScope.BeginLifetimeScope();
				var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
				return await mediator.Send(request, cancellationToken);
			}
			catch (Exception ex) when (ex is not TaskCanceledException)
			{
				LogException(ex, request);
				throw;
			}
		}

		public async Task Publish(
			object notification,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await using var scope = _lifetimeScope.BeginLifetimeScope();
				var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
				await mediator.Publish(notification, cancellationToken);
			}
			catch (Exception ex) when (ex is not TaskCanceledException)
			{
				LogException(ex, notification);
				throw;
			}
		}

		public async Task Publish<TNotification>(
			TNotification notification,
			CancellationToken cancellationToken = default)
			where TNotification : INotification
		{
			try
			{
				await using var scope = _lifetimeScope.BeginLifetimeScope();
				var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
				await mediator.Publish(notification, cancellationToken);
			}
			catch (Exception ex) when (ex is not TaskCanceledException)
			{
				LogException(ex, notification);
				throw;
			}
		}

		private void LogException<T>(Exception exception, T value)
		{
			var requestName = typeof(T).Name;
			var logger = _lifetimeScope.Resolve<ILogger<T>>();

			logger.LogError(
				exception,
				"Request: Unhandled Exception for {Name} {@Request}",
				requestName,
				value);
		}

		public class DoNotDecorate : Parameter
		{
			public override bool CanSupplyValue(
				ParameterInfo pi,
				IComponentContext context,
				[NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
			{
				// this is dummy parameter type used as a flag
				valueProvider = null;
				return false;
			}
		}
	}
}
