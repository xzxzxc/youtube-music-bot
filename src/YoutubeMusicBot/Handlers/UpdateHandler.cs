using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Extensions;

namespace YoutubeMusicBot.Handlers
{
    internal class UpdateHandler : IRequestHandler<UpdateHandler.Request, Unit>
    {
        private readonly IMediator _mediator;

        public UpdateHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await _mediator.Send(
                request.Update.Type switch
                {
                    UpdateType.Message =>
                        new MessageHandler.Request(request.Update.Message.ToContext()),
                    UpdateType.CallbackQuery =>
                        new CallbackQueryHandler.Request(request.Update.CallbackQuery.ToContext()),
                    _ => throw new ArgumentOutOfRangeException(),
                },
                cancellationToken);

            return Unit.Value;
        }

        public record Request(Update Update) : IRequest;
    }
}
