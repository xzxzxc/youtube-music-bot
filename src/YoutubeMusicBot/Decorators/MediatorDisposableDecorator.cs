using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;

namespace YoutubeMusicBot.Decorators
{
	internal class MediatorDisposableDecorator : IMediator
	{
		//private readonly IMediator _mediator;
		private readonly ILifetimeScope _lifetimeScope;

		public MediatorDisposableDecorator(
			//IMediator mediator,
			ILifetimeScope lifetimeScope)
		{
			//_mediator = mediator;
			_lifetimeScope = lifetimeScope;
		}

		public async Task<TResponse> Send<TResponse>(
			IRequest<TResponse> request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			// TODO: add try-catch
			await using var scope = BeginLifetimeScope();

			// we would get _mediator here
			var mediator = scope.Resolve<IMediator>(new DoNotDecorate());

			return await mediator.Send(request, cancellationToken);
		}

		public async Task<object?> Send(
			object request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			await using var scope = BeginLifetimeScope();
			var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
			return await mediator.Send(request, cancellationToken);
		}

		public async Task Publish(
			object notification,
			CancellationToken cancellationToken = new CancellationToken())
		{
			await using var scope = BeginLifetimeScope();
			var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
			await mediator.Publish(notification, cancellationToken);
		}

		public async Task Publish<TNotification>(
			TNotification notification,
			CancellationToken cancellationToken = new CancellationToken())
			where TNotification : INotification
		{
			await using var scope = BeginLifetimeScope();
			var mediator = scope.Resolve<IMediator>(new DoNotDecorate());
			await mediator.Publish(notification, cancellationToken);
		}

		private ILifetimeScope BeginLifetimeScope() =>
			_lifetimeScope.BeginLifetimeScope(
				//c => c.RegisterDecorator<IMediator>((_, _, _) => _mediator)
				);

		public class DoNotDecorate : Parameter
		{
			public override bool CanSupplyValue(
				ParameterInfo pi,
				IComponentContext context,
				out Func<object?>? valueProvider)
			{
				// this is dummy parameter type used as a flag
				valueProvider = null;
				return false;
			}
		}
	}
}
