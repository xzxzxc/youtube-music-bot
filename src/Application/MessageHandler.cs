using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using MediatR;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class MessageHandler : IRequestHandler<MessageHandler.Request, Unit>
    {
        private readonly IMessageScopeFactory _scopeFactory;
        private readonly IDbContext _dbContext;

        public MessageHandler(IMessageScopeFactory scopeFactory, IDbContext dbContext)
        {
            _scopeFactory = scopeFactory;
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var message = new Message { ExternalId = request.Value.Id, };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await using var messageScope = _scopeFactory.Create(
                request.Value);
            var internalHandler = messageScope.Resolve<Internal>();
            await internalHandler.HandleAsync(cancellationToken);

            return Unit.Value;
        }

        public class Internal : IAsyncDisposable
        {
            private readonly IMediator _mediator;
            private readonly MessageContext _messageContext;
            private readonly ITgClientWrapper _tgClientWrapper;
            private readonly IValidator<MessageModel> _validator;
            private readonly ICancellationRegistration _cancellationRegistration;
            private readonly IYoutubeDlWrapper _youtubeDlWrapper;
            private MessageModel? _messageToDeleteOnDispose = null;

            public Internal(
                IMediator mediator,
                MessageContext messageContext,
                ITgClientWrapper tgClientWrapper,
                IValidator<MessageModel> validator,
                ICancellationRegistration cancellationRegistration,
                IYoutubeDlWrapper youtubeDlWrapper)
            {
                _mediator = mediator;
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

                await foreach (var file in _youtubeDlWrapper.DownloadAsync(
                    _messageContext.UserMessage.Text,
                    cancellationToken))
                {
                    await _mediator.Send(
                        new NewTrackHandler.Request(file),
                        cancellationToken);
                }
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
