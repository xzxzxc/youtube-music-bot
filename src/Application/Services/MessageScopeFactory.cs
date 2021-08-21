using Autofac;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Services
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
