using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using MediatR;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Handlers
{
    internal class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
    {
        private readonly ILifetimeScope _lifetimeScope;

        public MessageHandler(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var messageScope = _lifetimeScope.BeginMessageLifetimeScope(
                request.Value);
            var internalHandler = messageScope.Resolve<Internal>();
            await internalHandler.HandleAsync(request.Value, cancellationToken);

            return Unit.Value;
        }

        public class Internal : IAsyncDisposable
        {
            private readonly ITgClientWrapper _tgClientWrapper;
            private readonly IValidator<MessageContext> _validator;
            private readonly ICancellationRegistration _cancellationRegistration;
            private readonly IYoutubeDlWrapper _youtubeDlWrapper;
            private MessageContext? _messageToDeleteOnDispose = null;

            public Internal(
                ITgClientWrapper tgClientWrapper,
                IValidator<MessageContext> validator,
                ICancellationRegistration cancellationRegistration,
                IYoutubeDlWrapper youtubeDlWrapper)
            {
                _tgClientWrapper = tgClientWrapper;
                _validator = validator;
                _cancellationRegistration = cancellationRegistration;
                _youtubeDlWrapper = youtubeDlWrapper;
            }

            public async Task HandleAsync(
                MessageContext message,
                CancellationToken cancellationToken = default)
            {
                var validationResult = await _validator.ValidateAsync(message, cancellationToken);

                if (!validationResult.IsValid)
                {
                    await _tgClientWrapper.SendMessageAsync(
                        string.Join('\n', validationResult.Errors),
                        cancellationToken: cancellationToken);
                    return;
                }

                using var cancellationProvider = _cancellationRegistration.RegisterNewProvider();

                var inlineButton = new InlineButton("Cancel", cancellationProvider.CallbackData);
                message.MessageToUpdate = _messageToDeleteOnDispose =
                    await _tgClientWrapper.SendMessageAsync(
                        "Loading started.",
                        inlineButton,
                        cancellationToken);

                var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    cancellationProvider.Token);

                await _youtubeDlWrapper.DownloadAsync(message.Text, cancellationSource.Token);
            }

            public async ValueTask DisposeAsync()
            {
                if (_messageToDeleteOnDispose != null)
                    await _tgClientWrapper.DeleteMessageAsync(_messageToDeleteOnDispose.Id);
            }
        }

        public record Request(MessageContext Value) : IRequest;
    }
}
