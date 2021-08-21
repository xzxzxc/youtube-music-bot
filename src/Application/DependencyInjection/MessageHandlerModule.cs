using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YoutubeMusicBot.Console.Handlers;

namespace YoutubeMusicBot.Console.DependencyInjection
{
    public class MessageHandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddValidatorsFromAssembly(ThisAssembly);

            builder.Populate(serviceCollection);

            builder.RegisterType<MessageHandler.Internal>();

            base.Load(builder);
        }
    }
}
