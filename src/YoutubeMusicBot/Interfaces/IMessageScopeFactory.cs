using Autofac;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Interfaces
{
    public interface IMessageScopeFactory
    {
        ILifetimeScope Create(MessageModel messageModel);
    }
}
