using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using YoutubeMusicBot.Handlers;

namespace YoutubeMusicBot.IntegrationTests
{
	public class RunProcessHandlerTests
	{
		[Test]
		[AutoData]
		public async Task ShouldEcho(string message)
		{
			using var host = Program.CreateHostBuilder()
				.UseSerilog(new LoggerConfiguration().WriteTo.InMemory().CreateLogger())
				.Build();
			var runProcessHandler = host.Services.GetRequiredService<RunProcessHandler>();
			string outputLine = string.Empty;

			await runProcessHandler.Handle(
				new RunProcessHandler.Request(
					"echo",
					WorkingDirectory: ".",
					async (line, _) => outputLine = line,
					Arguments: message));

			InMemorySink.Instance.LogEvents.Should()
				.NotContain(e => e.Level >= LogEventLevel.Error);
			outputLine.Should().Be(message);
		}
	}
}
