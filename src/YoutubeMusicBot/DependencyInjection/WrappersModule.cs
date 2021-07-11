using Autofac;
using YoutubeMusicBot.Wrappers;

namespace YoutubeMusicBot.DependencyInjection
{
    public class WrappersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TgClientWrapper>()
                .AsImplementedInterfaces();

            builder.RegisterType<YoutubeDlWrapper>()
                .AsImplementedInterfaces();

            builder.RegisterType<Mp3SplitWrapper>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
