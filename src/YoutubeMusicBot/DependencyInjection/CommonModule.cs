using Autofac;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Services;

namespace YoutubeMusicBot.DependencyInjection
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageScopeFactory>()
                .AsImplementedInterfaces();

            builder.RegisterType<ProcessRunner>()
                .AsImplementedInterfaces();

            builder.RegisterType<LinuxPathResolver>()
                .AsImplementedInterfaces();

            builder.RegisterType<DescriptionService>()
                .AsImplementedInterfaces();

            builder.RegisterType<YoutubeDlConfigPath>()
                .AsImplementedInterfaces()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
