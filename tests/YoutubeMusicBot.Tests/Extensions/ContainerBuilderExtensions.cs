using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extras.Moq;
using MediatR.Pipeline;
using Microsoft.Extensions.Options;
using Moq;

namespace YoutubeMusicBot.Tests.Extensions
{
	public static class ContainerBuilderExtensions
	{
		public static ContainerBuilder RegisterDefaultOptions<TOptions>(
			this ContainerBuilder builder)
			where TOptions : class, new() =>
			builder.RegisterOptions(new TOptions());

		public static ContainerBuilder RegisterOptions<TOptions>(
			this ContainerBuilder builder,
			TOptions options)
			where TOptions : class
		{
			var redisOptionsMock = new Mock<IOptionsMonitor<TOptions>>
			{
				DefaultValue = DefaultValue.Mock,
			};
			redisOptionsMock
				.SetupGet(m => m.CurrentValue)
				.Returns(options);
			builder.RegisterMock(redisOptionsMock);

			return builder;
		}
	}
}
