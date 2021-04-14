using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace YoutubeMusicBot.Behaviour
{
	public interface INotificationBehavior<TNotification>
		where TNotification : INotification
	{
		Task Handle(
			TNotification notification,
			CancellationToken cancellationToken,
			Func<Task> next);
	}
}
