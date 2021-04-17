using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	internal class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
	{
		private readonly ILifetimeScope _lifetimeScope;

		public MessageHandler(
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public async Task<Unit> Handle(
			Request request,
			CancellationToken cancellationToken = default)
		{
			var message = request.Value
				?? throw new ArgumentNullException(nameof(request));

			var messageContext = message.ToContext();

			await using var messageScope =
				_lifetimeScope.BeginMessageLifetimeScope(
					messageContext);

			// TODO: add validation

			await messageScope.Resolve<IYoutubeDlWrapper>()
				.DownloadAsync(
					message.Text,
					cancellationToken);

			return Unit.Value;
		}

		public record Request(Telegram.Bot.Types.Message? Value) : IRequest
		{
		}
	}
}
