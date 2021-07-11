using Autofac;
using YoutubeMusicBot.Services;

namespace YoutubeMusicBot.DependencyInjection
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageScopeFactory>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
