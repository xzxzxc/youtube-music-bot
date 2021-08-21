using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Infrastructure.Extensions;

namespace YoutubeMusicBot.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();

            await host.Services.GetRequiredService<ApplicationDbContext>()
                .Database.MigrateAsync();

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

            serviceCollection.AddOptions<DownloadOptions>().BindConfiguration("Download");
            serviceCollection.AddOptions<BotOptions>().BindConfiguration("Bot");

            builder.Populate(serviceCollection);
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
