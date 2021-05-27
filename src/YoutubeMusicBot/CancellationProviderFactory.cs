using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    // TODO: remove this shit
    internal class CancellationProviderFactory : ICancellationProviderFactory
    {
        private readonly MessageContext _messageContext;
        private readonly ICancellationRegistration _cancellationRegistration;

        public CancellationProviderFactory(
            MessageContext messageContext,
            ICancellationRegistration cancellationRegistration)
        {
            _messageContext = messageContext;
            _cancellationRegistration = cancellationRegistration;
        }

        public CancellationProvider Create() =>
            // _messageContext.CancellationProvider =
                _cancellationRegistration.RegisterNewProvider();
    }
}
