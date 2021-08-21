using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain.Enums;

namespace YoutubeMusicBot.Application
{
    public class CallbackQueryHandler : IRequestHandler<CallbackQueryHandler.Request, Unit>
    {
        private readonly ICallbackFactory _callbackFactory;
        private readonly ICancellationRegistration _cancellationRegistration;

        public CallbackQueryHandler(
            ICallbackFactory callbackFactory,
            ICancellationRegistration cancellationRegistration)
        {
            _callbackFactory = callbackFactory;
            _cancellationRegistration = cancellationRegistration;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var callbackData = request.Value.CallbackData
                ?? throw new ArgumentOutOfRangeException(
                    nameof(request),
                    request,
                    "Callback data is null");

            var action = _callbackFactory.GetActionFromData(callbackData);
            switch (action)
            {
                case CallbackAction.Cancel:
                    _cancellationRegistration.GetProvider(callbackData).Cancel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(action),
                        action,
                        "Unknown action value");
            }

            return Unit.Value;
        }

        public record Request(CallbackQueryContext Value) : IRequest;
    }
}
