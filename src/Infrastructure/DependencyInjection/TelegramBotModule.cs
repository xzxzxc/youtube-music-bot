using System;
using Autofac;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using YoutubeMusicBot.Console.Options;

namespace YoutubeMusicBot.Console.DependencyInjection
{
    public class TelegramBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(
                    ctx =>
                    {
                        var botOptions = ctx
                            .Resolve<IOptionsMonitor<BotOptions>>()
                            .CurrentValue;
                        return new TelegramBotClient(
                            botOptions.Token
                            ?? throw new InvalidOperationException(
                                "Bot token must be not empty!"));
                    })
                .As<ITelegramBotClient>()
                .SingleInstance();
        }
    }
}
