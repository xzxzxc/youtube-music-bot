using Autofac;
using YoutubeMusicBot.Application.Services;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TrackListParser>()
                .AsImplementedInterfaces();
        }
    }
}
