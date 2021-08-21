using Autofac;
using YoutubeMusicBot.Application;

namespace YoutubeMusicBot.Infrastructure.DependencyInjection
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProcessRunner>()
                .AsImplementedInterfaces();

            builder.RegisterType<LinuxPathResolver>()
                .AsImplementedInterfaces();

            builder.RegisterType<DescriptionService>()
                .AsImplementedInterfaces();

            builder.RegisterType<YoutubeDlConfigPath>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<CacheFolder>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
