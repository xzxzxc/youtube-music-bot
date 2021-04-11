using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using YoutubeMusicBot.Behaviour;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
	internal class NewTrackHandler :
		INotificationHandler<NewTrackHandler.Notification>
	{
		private readonly ITgClientWrapper _tgClientWrapper;

		public NewTrackHandler(ITgClientWrapper tgClientWrapper)
		{
			_tgClientWrapper = tgClientWrapper;
		}

		public async Task Handle(
			Notification notification,
			CancellationToken cancellationToken)
		{
			var file = notification.File;

			// TODO: add retry policy
			try
			{
				await _tgClientWrapper.SendAudioAsync(notification.Chat, file);
			}
			finally
			{
				file.Delete();
			}
		}

		public record Notification(
			ChatContext Chat,
			FileInfo File) : INotification
		{
		}
	}

	public class MediatorDecorator : IMediator
	{
		private readonly IMediator _mediator;
		private readonly ILifetimeScope _lifetimeScope;

		public MediatorDecorator(
			IMediator mediator,
			ILifetimeScope lifetimeScope)
		{
			_mediator = mediator;
			_lifetimeScope = lifetimeScope;
		}

		public async Task<TResponse> Send<TResponse>(
			IRequest<TResponse> request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return await _mediator.Send(request, cancellationToken);
		}

		public async Task<object?> Send(
			object request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return await _mediator.Send(request, cancellationToken);
		}

		public async Task Publish(
			object notification,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public async Task Publish<TNotification>(
			TNotification notification,
			CancellationToken cancellationToken = new CancellationToken())
			where TNotification : INotification
		{
			Func<Task> handler = () => _mediator.Publish(
				notification,
				cancellationToken);
			await _lifetimeScope
				.Resolve<IEnumerable<INotificationBehavior<TNotification>>>()
				.Reverse()
				.Aggregate(
					handler,
					(next, pipeline) => () => pipeline.Handle(
						notification,
						cancellationToken,
						next))();
		}
	}
}
