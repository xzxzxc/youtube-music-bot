using Autofac;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using YoutubeMusicBot.Application.Options;

namespace YoutubeMusicBot.Infrastructure.DependencyInjection
{
    public class TelegramBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(
                    ctx =>
                    {
                        var botOptions = ctx.Resolve<IOptions<BotOptions>>().Value;
                        return new TelegramBotClient(botOptions.Token);
                    })
                .As<ITelegramBotClient>()
                .SingleInstance();

            builder.RegisterType<TgClient>()
                .AsImplementedInterfaces();
        }
    }
}
