using Autofac;
using YoutubeMusicBot.Console.Wrappers;

namespace YoutubeMusicBot.Console.DependencyInjection
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
