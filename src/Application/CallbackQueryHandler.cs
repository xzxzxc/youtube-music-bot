using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Domain.Enums;

namespace YoutubeMusicBot.Application
{
    public class CallbackQueryHandler : IRequestHandler<CallbackQueryHandler.Request>
    {
        private readonly ICallbackFactory _callbackFactory;
        private readonly ICancellationRegistration _cancellationRegistration;
        private readonly ICallbackDataFactory _callbackDataFactory;
        private readonly IOptionsMonitor<FeatureOptions> _options;
        private readonly IMediator _mediator;

        public CallbackQueryHandler(
            ICallbackFactory callbackFactory,
            ICancellationRegistration cancellationRegistration,
            ICallbackDataFactory callbackDataFactory,
            IOptionsMonitor<FeatureOptions> options,
            IMediator mediator)
        {
            _callbackFactory = callbackFactory;
            _cancellationRegistration = cancellationRegistration;
            _callbackDataFactory = callbackDataFactory;
            _options = options;
            _mediator = mediator;
        }

        public async ValueTask Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            var callbackData = request.Value.CallbackData
                ?? throw new ArgumentOutOfRangeException(
                    nameof(request),
                    request,
                    "Callback data is null");

            if (_options.CurrentValue.EsArchitectureEnabled)
            {
                var parseResult = _callbackDataFactory.Parse(request.Value.CallbackData);
                switch (parseResult)
                {
                    case CancelResult cancelResult:
                        _mediator.Cancel(cancelResult.EventCancellationId);
                        break;
                    default:
                        var type = parseResult.GetType();
                        throw new InvalidOperationException(
                            $"Unknown parse result type {type.FullName ?? type.Name}");
                }

                return;
            }

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
        }

        public record Request(CallbackQueryContext Value) : IRequest;
    }
}
