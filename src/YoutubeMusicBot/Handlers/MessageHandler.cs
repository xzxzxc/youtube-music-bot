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

namespace YoutubeMusicBot.Handlers
{
    public class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IValidator<MessageContext> _validator;

        public MessageHandler(ILifetimeScope lifetimeScope, IValidator<MessageContext> validator)
        {
            _lifetimeScope = lifetimeScope;
            _validator = validator;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var message = request.Value;

            await using var messageScope = _lifetimeScope.BeginMessageLifetimeScope(message);

            // TODO: remove ugly resolves
            var validationResult = await _validator.ValidateAsync(message, cancellationToken);
            var tgClientWrapper = messageScope.Resolve<ITgClientWrapper>();

            if (!validationResult.IsValid)
            {
                await tgClientWrapper.SendMessageAsync(
                    string.Join('\n', validationResult.Errors),
                    cancellationToken: cancellationToken);
                return Unit.Value;
            }

            var cancellationRegistration = messageScope.Resolve<ICancellationRegistration>();

            using var cancellationProvider = cancellationRegistration.RegisterNewProvider();

            await tgClientWrapper.SendMessageAsync(
                "Loading started.",
                new InlineButton("Cancel", cancellationProvider.Str),
                cancellationToken);

            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                cancellationProvider.Token);

            var youtubeDlWrapper = messageScope.Resolve<IYoutubeDlWrapper>();

            await youtubeDlWrapper
                .DownloadAsync(
                    message.Text,
                    cancellationSource.Token);

            return Unit.Value;
        }

        public record Request(MessageContext Value) : IRequest;

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
