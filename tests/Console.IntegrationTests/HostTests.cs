using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Console.IntegrationTest
{
    [Parallelizable]
	public class HostTests
	{
		[Test]
		[Timeout(2_000)] // 2 sec
		public async Task ShouldGracefullyShutDown()
		{
			var host = Host.CreateDefaultBuilder()
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureContainer<ContainerBuilder>(
					(context, builder) =>
					{
						Program.ConfigureContainer(context, builder);
						builder.RegisterOptions(
							new BotOptions
							{
								Token = Secrets.BotToken
							});
					})
				.Build();
			var hostLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
			var tokenSource = new CancellationTokenSource();
			hostLifetime.ApplicationStarted.Register(
				() => tokenSource.Cancel());

			await host.RunAsync(tokenSource.Token);
		}
	}
}
