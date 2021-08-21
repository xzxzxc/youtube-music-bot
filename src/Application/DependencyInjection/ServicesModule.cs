using System.Linq;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using YoutubeMusicBot.Console.Mediatr;
using YoutubeMusicBot.Console.Services;

namespace YoutubeMusicBot.Console.Handlers.DependencyInjection
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<CallbackFactory>()
                .AsImplementedInterfaces();

            builder.RegisterType<MessageScopeFactory>()
                .AsImplementedInterfaces();

            builder.RegisterType<CancellationRegistration>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
