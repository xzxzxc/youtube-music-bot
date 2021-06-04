using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Mediatr;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Wrappers;

namespace YoutubeMusicBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(params string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureConfiguration)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                .UseSerilog(
                    (ctx, config) =>
                        config.ReadFrom.Configuration(ctx.Configuration));

        public static void ConfigureContainer(
            HostBuilderContext _,
            ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<CancellationRegistration>()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder.RegisterType<TgClientWrapper>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<YoutubeDlWrapper>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<Mp3SplitWrapper>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<CacheFolder>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<TrackListParser>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<RunProcessHandler>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<CallbackFactory>()
                .AsImplementedInterfaces();

            containerBuilder.RegisterType<MessageHandler.Internal>();

            containerBuilder.Register(
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

            containerBuilder.RegisterMediatR(Assembly.GetExecutingAssembly());

            containerBuilder.RegisterDecorator<IMediator>(
                (context, parameters, instance) =>
                {
                    if (parameters
                        .OfType<MediatorDisposableDecorator.DoNotDecorate>()
                        .Any())
                    {
                        return instance;
                    }

                    return new MediatorDisposableDecorator(
                        context.Resolve<ILifetimeScope>());
                });

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            serviceCollection.AddHostedService<BotHostedService>();
            serviceCollection.AddOptions<DownloadOptions>()
                .BindConfiguration("Download");
            serviceCollection.AddOptions<BotOptions>().BindConfiguration("Bot");

            containerBuilder.Populate(serviceCollection);
        }

        private static void ConfigureConfiguration(
            HostBuilderContext _,
            IConfigurationBuilder builder)
        {
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.AddJsonFile("appsettings.Secrets.json");
        }
    }
}
