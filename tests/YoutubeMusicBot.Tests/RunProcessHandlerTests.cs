using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using YoutubeMusicBot.Tests.Stubs;

namespace YoutubeMusicBot.Tests
{
	public class RunProcessHandlerTests
	{
		[Test]
		[AutoData]
		public async Task ShouldEcho(string message)
		{
			LogsHolder.Executions.Clear();
			using var autoMock = AutoMock.GetStrict(
				builder =>
				{
					Program.ConfigureContainer(null, builder);
					builder.RegisterGeneric(typeof(LoggerStub<>))
						.As(typeof(ILogger<>));
				});
			var runProcessHandler = autoMock.Create<RunProcessHandler>();
			string outputLine = string.Empty;

			await runProcessHandler.Handle(
				new RunProcessHandler.Request(
					"echo",
					WorkingDirectory: ".",
					async line => outputLine = line,
					Arguments: message));

			LogsHolder.Executions.Should()
				.NotContain(e => e.LogLevel >= LogLevel.Error);
			outputLine.Should().Be(message);
		}
	}
}
