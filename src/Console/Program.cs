using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Infrastructure.Extensions;
using YoutubeMusicBot.Infrastructure.Options;

namespace YoutubeMusicBot.Console
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args)
                .Build();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(params string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices(ConfigureServices)
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
        }

        private static void ConfigureServices(IServiceCollection collection)
        {
            collection.AddOptionsFromConfiguration<FileSystemOptions>("Download");
            collection.AddOptionsFromConfiguration<BotOptions>("Bot");
            collection.AddOptionsFromConfiguration<SplitOptions>("Split");


            collection.AddHostedService<MigrationHostedService>();
            collection.AddRepositoryInitializers();
            collection.AddHostedService<BotHostedService>();
        }

        private static void AddOptionsFromConfiguration<TOptions>(
            this IServiceCollection collection,
            string configSectionPath)
            where TOptions : class =>
            collection.AddOptions<TOptions>()
                .BindConfiguration(configSectionPath)
                .ValidateDataAnnotations();

        private static void AddRepositoryInitializers(this IServiceCollection services)
        {
            var addInitializerOpenMethod = typeof(Program)
                .GetMethod(
                    nameof(AddRepositoryInitializer),
                    BindingFlags.Static | BindingFlags.NonPublic)!;
            foreach (var aggregateType in EventSourcingModule.GetAggregateTypes())
            {
                addInitializerOpenMethod.MakeGenericMethod(aggregateType)
                    .Invoke(null, new object?[] { services });
            }
        }

        private static IServiceCollection AddRepositoryInitializer<TAggregate>(
            this IServiceCollection services)
            where TAggregate : AggregateBase<TAggregate> =>
            services.AddHostedService<RepositoryInitializer<TAggregate>>();
    }
}
