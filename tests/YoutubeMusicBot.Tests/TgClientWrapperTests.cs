using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.ResolveAnything;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Tests.Extensions;
using YoutubeMusicBot.Wrappers;

namespace YoutubeMusicBot.Tests
{
	[Parallelizable]
	public class TgClientWrapperTests
	{
		[Test]
		[Timeout(120_000)] // 2 minutes
		[InlineAutoData(Secrets.GroupChatId, 21)]
		public async Task ShouldHandleFloodControl(
			long chatId,
			int messagesCount,
			string messageText)
		{
			using var host = Program.CreateHostBuilder()
				.ConfigureContainer<ContainerBuilder>(
					(_, builder) => builder.RegisterOptions(
							new BotOptions
							{
								Token = Secrets.BotToken
							})
						.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource()))
				.Build();
			var fixture = AutoFixtureFactory.Create();
			var chat = fixture
				.Build<ChatContext>()
				.With(c => c.Id, chatId)
				.Create();
			var message = fixture
				.Build<MessageContext>()
				.With(m => m.Chat, chat)
				.Create();
			await using var scope = host.Services.GetRequiredService<ILifetimeScope>()
				.BeginMessageLifetimeScope(message);

			var wrapper = scope.Resolve<TgClientWrapper>();

			var responses = await Task.WhenAll(
				Enumerable.Range(0, messagesCount)
					.Select(_ => wrapper.SendMessageAsync(messageText)));

			responses.Should().NotContainNulls();
		}
	}
}
