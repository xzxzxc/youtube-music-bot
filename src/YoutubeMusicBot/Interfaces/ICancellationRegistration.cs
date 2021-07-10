namespace YoutubeMusicBot.Interfaces
{
    public interface ICancellationRegistration
    {
        /// <summary>
        /// Find provider by <see cref="id"/>.
        /// </summary>
        ICancellationProvider GetProvider(string id);


        /// <summary>
        /// Create and register provider that could be used to cancel command.
        /// <remarks>Created provider need to be unregistered using Dispose method.</remarks>
        /// </summary>
        ICancellationProvider RegisterNewProvider();
    }
}
