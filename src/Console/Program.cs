using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Infrastructure.Extensions;
using YoutubeMusicBot.Infrastructure.Options;

namespace YoutubeMusicBot.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();

            await Initialize(host);

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
            ContainerBuilder builder)
        {
            builder.RegisterApplicationModules();
            builder.RegisterInfrastructureModules();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            builder.RegisterType<BotUpdatesProcessor>();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHostedService<BotHostedService>();

            serviceCollection.AddOptions<FileSystemOptions>().BindConfiguration("Download");
            serviceCollection.AddOptions<BotOptions>().BindConfiguration("Bot");
            serviceCollection.AddOptions<SplitOptions>().BindConfiguration("Split");

            builder.Populate(serviceCollection);
        }

        public static async Task Initialize(IHost host)
        {
            var initializables = host.Services.GetServices<IInitializable>();

            foreach (var initializable in initializables.OrderBy(i => i.Order))
                await initializable.Initialize();
        }

        private static void ConfigureConfiguration(
            HostBuilderContext _,
            IConfigurationBuilder builder)
        {
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
        }
    }
}
