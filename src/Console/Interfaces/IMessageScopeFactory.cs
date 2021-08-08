using Autofac;
using YoutubeMusicBot.Console.Models;

namespace YoutubeMusicBot.Console.Interfaces
{
    public interface IMessageScopeFactory
    {
        ILifetimeScope Create(MessageModel messageModel);
    }
}
