using Autofac;
using YoutubeMusicBot.Console.Interfaces;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Services
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
