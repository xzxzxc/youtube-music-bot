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

		public static ContainerBuilder RegisterMediatrDependenciesMocks(
			this ContainerBuilder builder)
		{
			builder.RegisterOpenGenericMock(
				typeof(IRequestPreProcessor<>));
			builder.RegisterOpenGenericMock(
				typeof(IRequestExceptionAction<,>));
			builder.RegisterOpenGenericMock(
				typeof(IRequestExceptionHandler<,,>));
			builder.RegisterOpenGenericMock(
				typeof(IRequestPostProcessor<,>));

			return builder;
		}

		public static ContainerBuilder RegisterOpenGenericMock(
			this ContainerBuilder builder,
			Type openGenericType,
			MockBehavior mockBehavior = MockBehavior.Default,
			DefaultValue defaultValue = DefaultValue.Empty)
		{
			builder.RegisterGeneric(
					(_, typeArguments) =>
					{
						// for example openGenericType is T<>
						// closedGeneric would be T<Arg1, Arg2, ...>
						var closedGeneric = openGenericType
							.MakeGenericType(typeArguments);
						// Mock<T<Arg1, Arg2, ...>>
						var mockType = typeof(Mock<>).MakeGenericType(
							closedGeneric);
						// setter of Mock<T<Arg1, Arg2, ...>>.DefaultValue
						var defaultValuePropSetter = mockType.GetProperty(
									nameof(Mock<object>.DefaultValue))
								?.GetSetMethod()
							?? throw new InvalidOperationException(); // TODO:
						// instance of Mock<T<Arg1, Arg2, ...>>
						var mock = Activator.CreateInstance(
								mockType,
								mockBehavior)
							?? throw new InvalidOperationException(); // TODO:
						// mock.DefaultValue = defaultValue;
						defaultValuePropSetter.Invoke(
							mock,
							new object[] { defaultValue });

						// return mock.Object;
						return mockType.GetProperty(
									nameof(Mock<object>.Object),
									returnType: closedGeneric)
								?.GetGetMethod()
								?.Invoke(mock, parameters: null)
							?? throw new InvalidOperationException(); // TODO:
					})
				.As(openGenericType);

			return builder;
		}
	}
}
