using Autofac;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IMessageScopeFactory
    {
        ILifetimeScope Create(MessageModel messageModel);
    }
}
