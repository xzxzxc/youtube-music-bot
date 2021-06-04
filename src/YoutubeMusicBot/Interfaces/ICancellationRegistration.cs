namespace YoutubeMusicBot.Interfaces
{
    public interface ICancellationRegistration
    {
        /// <summary>
        /// Find provider by <see cref="callbackData"/>.
        /// </summary>
        ICancellationProvider GetProvider(string callbackData);


        /// <summary>
        /// Create and register provider that could be used to cancel command.
        /// <remarks>Created provider need to be unregistered using Dispose method.</remarks>
        /// </summary>
        ICancellationProvider RegisterNewProvider();
    }
}
