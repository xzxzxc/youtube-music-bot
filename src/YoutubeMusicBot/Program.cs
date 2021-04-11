using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using YoutubeMusicBot.Behaviour;

namespace YoutubeMusicBot
{
	internal class Program
	{
		public static async Task Main(string[] args)
		{
			var host = CreateHostBuilder(args)
				.Build();

			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(params string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureContainer<ContainerBuilder>(ConfigureContainer)
				.UseSerilog(
					(ctx, config) =>
						config.ReadFrom.Configuration(ctx.Configuration));

		public static void ConfigureContainer(
			HostBuilderContext? _,
			ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterType<YoutubeDlWrapper>()
				.AsImplementedInterfaces();

			containerBuilder.RegisterType<TgClientWrapper>()
				.AsImplementedInterfaces();

			containerBuilder.RegisterType<TrackFilesWatcher>()
				.AsImplementedInterfaces()
				.SingleInstance();

			containerBuilder.Register(
					ctx =>
					{
						var botOptions = ctx
							.Resolve<IOptionsMonitor<BotOptions>>()
							.CurrentValue;
						return new TelegramBotClient(botOptions.Token);
					})
				.As<ITelegramBotClient>()
				.SingleInstance();

			containerBuilder.RegisterMediatR(Assembly.GetExecutingAssembly());
			// exception handler must be the last one
			containerBuilder.RegisterGeneric(
				typeof(UnhandledExceptionBehaviour<>))
				.AsImplementedInterfaces();
			containerBuilder.RegisterDecorator<MediatorDecorator, IMediator>();

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddHostedService<BotHostedService>();
			serviceCollection.AddOptions<DownloadOptions>()
				.BindConfiguration("Download");
			serviceCollection.AddOptions<BotOptions>().BindConfiguration("Bot");

			containerBuilder.Populate(serviceCollection);
		}
	}
}
