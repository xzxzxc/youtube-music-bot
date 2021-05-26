using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using MediatR;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot
{
	public class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
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

			await using var messageScope = _lifetimeScope.BeginMessageLifetimeScope(message);

			var validationResult = await messageScope.Resolve<IValidator<MessageContext>>()
				.ValidateAsync(message, cancellationToken);

			if (!validationResult.IsValid)
			{
				var tgClientWrapper = _lifetimeScope.Resolve<ITgClientWrapper>();

				await tgClientWrapper.SendMessageAsync(
					string.Join('\n', validationResult.Errors),
					cancellationToken);
				return Unit.Value;
			}

			await messageScope.Resolve<IYoutubeDlWrapper>()
				.DownloadAsync(
					message.Text,
					cancellationToken);

			return Unit.Value;
		}

		public record Request(MessageContext? Value) : IRequest
		{
		}

		public class MessageContextValidator : AbstractValidator<MessageContext>
		{
			private static readonly string[] AllowedSchemes = { "http", "https", "ftp" };

			public MessageContextValidator()
			{
				RuleFor(r => r.Text)
					.NotEmpty()
					.WithMessage("Message must be not empty.")
					.DependentRules(
						() =>
						{
							RuleFor(r => r.Text)
								.Must(
									m => Uri.TryCreate(m, UriKind.Absolute, out var uri)
										&& AllowedSchemes.Contains(uri.Scheme))
								.WithMessage("Message must be valid URL.");
						});
			}
		}
	}
}
