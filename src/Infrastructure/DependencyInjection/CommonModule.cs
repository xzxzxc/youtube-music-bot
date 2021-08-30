using Autofac;

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

            builder.RegisterType<YoutubeDlConfigPath>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<FileSystem>()
                .AsImplementedInterfaces();

            builder.RegisterType<MusicDownloader>()
                .AsImplementedInterfaces();

            builder.RegisterType<MusicSplitter>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
