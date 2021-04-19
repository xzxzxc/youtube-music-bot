using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
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
		[InlineAutoData(Secrets.BotToken, Secrets.GroupChatId, 21)]
		public async Task ShouldHandleFloodControl(
			string botToken,
			long chatId,
			int messagesCount,
			string message)
		{
			using var autoMock = AutoMock.GetStrict(
				builder =>
				{
					Program.ConfigureContainer(null, builder);
					builder.RegisterInstance(new MessageContext(
						new ChatContext(chatId),
						message));
					builder.RegisterOptions(
						new BotOptions
						{
							Token = botToken
						});
				});
			var wrapper = autoMock.Create<TgClientWrapper>();

			var responses = await Task.WhenAll(
				Enumerable.Range(0, messagesCount)
					.Select(_ => wrapper.SendMessageAsync(message)));

			responses.Should().NotContainNulls();
		}
	}
}
