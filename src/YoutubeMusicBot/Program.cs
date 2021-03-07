using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
				.ConfigureServices(ConfigureServices)
				.ConfigureContainer<ContainerBuilder>(ConfigureContainer);

		public static void ConfigureContainer(
			HostBuilderContext? _,
			ContainerBuilder containerBuilder)
		{
		}

		public static void ConfigureServices(
			HostBuilderContext _,
			IServiceCollection serviceCollection)
		{
			serviceCollection.AddHostedService<BotHostedService>();
		}
	}
}
