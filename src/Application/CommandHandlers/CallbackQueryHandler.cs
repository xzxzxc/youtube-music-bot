using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.CommandHandlers
{
    public class CallbackQueryHandler : ICommandHandler<CallbackQueryHandler.Command>
    {
        private readonly ICallbackDataFactory _callbackDataFactory;
        private readonly IMediator _mediator;

        public CallbackQueryHandler(
            ICallbackDataFactory callbackDataFactory,
            IMediator mediator)
        {
            _callbackDataFactory = callbackDataFactory;
            _mediator = mediator;
        }

        public async ValueTask Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var callbackData = command.CallbackData
                ?? throw new ArgumentOutOfRangeException(
                    nameof(command),
                    command,
                    "Callback data is null");

            var parseResult = _callbackDataFactory.Parse(callbackData);
            switch (parseResult)
            {
                case CancelResult<Message> cancelMessageResult:
                    await _mediator.Send(
                        new CancelMessageHandler.Command(cancelMessageResult.AggregateId),
                        cancellationToken);
                    break;
                default:
                    var type = parseResult.GetType();
                    throw new InvalidOperationException(
                        $"Unknown parse result type {type.FullName ?? type.Name}");
            }
        }

        public record Command(string? CallbackData) : ICommand;
    }
}
