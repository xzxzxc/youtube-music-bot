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
    public class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
    {
        private readonly IMessageScopeFactory _scopeFactory;

        public MessageHandler(IMessageScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            await using var messageScope = _scopeFactory.Create(
                request.Value);
            var internalHandler = messageScope.Resolve<Internal>();
            await internalHandler.HandleAsync(cancellationToken);

            return Unit.Value;
        }

        public class Internal : IAsyncDisposable
        {
            private readonly MessageContext _messageContext;
            private readonly ITgClientWrapper _tgClientWrapper;
            private readonly IValidator<MessageModel> _validator;
            private readonly ICancellationRegistration _cancellationRegistration;
            private readonly IYoutubeDlWrapper _youtubeDlWrapper;
            private MessageModel? _messageToDeleteOnDispose = null;

            public Internal(
                MessageContext messageContext,
                ITgClientWrapper tgClientWrapper,
                IValidator<MessageModel> validator,
                ICancellationRegistration cancellationRegistration,
                IYoutubeDlWrapper youtubeDlWrapper)
            {
                _messageContext = messageContext;
                _tgClientWrapper = tgClientWrapper;
                _validator = validator;
                _cancellationRegistration = cancellationRegistration;
                _youtubeDlWrapper = youtubeDlWrapper;
            }

            public async Task HandleAsync(CancellationToken cancellationToken = default)
            {
                var validationResult = await _validator.ValidateAsync(
                    _messageContext.UserMessage,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    await _tgClientWrapper.SendMessageAsync(
                        string.Join('\n', validationResult.Errors),
                        cancellationToken: cancellationToken);
                    return;
                }

                using var cancellationProvider = _cancellationRegistration.RegisterNewProvider(
                    cancellationToken);

                var inlineButton = new InlineButton("Cancel", cancellationProvider.CallbackData);
                _messageContext.MessageToUpdate = _messageToDeleteOnDispose =
                    await _tgClientWrapper.SendMessageAsync(
                        "Loading started.",
                        inlineButton,
                        cancellationToken);

                cancellationToken = cancellationProvider.Token;

                await _youtubeDlWrapper.DownloadAsync(
                    _messageContext.UserMessage.Text,
                    cancellationToken);
            }

            public async ValueTask DisposeAsync()
            {
                if (_messageToDeleteOnDispose != null)
                    await _tgClientWrapper.DeleteMessageAsync(_messageToDeleteOnDispose.Id);
            }
        }

        public record Request(MessageModel Value) : IRequest;
    }
}
