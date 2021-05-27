using System.Threading;
using System.Threading.Tasks;
using MediatR;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Handlers
{
    internal class CallbackQueryHandler : IRequestHandler<CallbackQueryHandler.Request, Unit>
    {
        private readonly ICancellationRegistration _cancellationRegistration;

        public CallbackQueryHandler(ICancellationRegistration cancellationRegistration)
        {
            _cancellationRegistration = cancellationRegistration;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var callbackData = request.CallbackQuery.CallbackData;
            if (callbackData != null
                && CancellationProvider.TryGetId(callbackData, out var id))
            {
                _cancellationRegistration.GetProvider(id).Cancel();
            }

            return Unit.Value;
        }

        internal record Request(CallbackQueryContext CallbackQuery) : IRequest;
    }
}
