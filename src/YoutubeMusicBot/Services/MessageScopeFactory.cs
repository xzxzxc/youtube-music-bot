using Autofac;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Services
{
    public class MessageScopeFactory : IMessageScopeFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public MessageScopeFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public ILifetimeScope Create(MessageModel messageModel)
        {
            return _lifetimeScope.BeginLifetimeScope(
                c => c.RegisterInstance(new MessageContext(messageModel)));
        }
    }
}
