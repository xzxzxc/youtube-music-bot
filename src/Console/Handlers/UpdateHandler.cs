using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Console.Extensions;

namespace YoutubeMusicBot.Console.Handlers
{
    public class UpdateHandler : IRequestHandler<UpdateHandler.Request, Unit>
    {
        private readonly IMediator _mediator;

        public UpdateHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            switch (request.Update.Type)
            {
                case UpdateType.Message:
                    await _mediator.Send(
                        new MessageHandler.Request(request.Update.Message.ToModel()),
                        cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    await _mediator.Send(
                        new CallbackQueryHandler.Request(request.Update.CallbackQuery.ToContext()),
                        cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Unit.Value;
        }

        public record Request(Update Update) : IRequest;
    }
}
