using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using IntegrationTests.Common;
using NUnit.Framework;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.Infrastructure.Wrappers;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Infrastructure.IntegrationTests
{
    [Parallelizable]
    public class TgClientWrapperTests
    {
        [Test]
        [Timeout(120_000)] // 2 minutes
        [InlineAutoData(Secrets.GroupChatIdForBot, 21)]
        public async Task ShouldHandleFloodControl(
            long chatId,
            int messagesCount)
        {
            var fixture = AutoFixtureFactory.Create();
            fixture.Freeze<ChatModel>(b => b.With(c => c.Id, chatId));
            var messageContext = fixture.Build<MessageContext>()
                .Create();
            using var container = CreateContainer(messageContext);
            var sut = container.Create<TgClientWrapper>();

            var responses = await Task.WhenAll(
                Enumerable.Range(0, messagesCount)
                    .Select(_ => sut.SendMessageAsync(messageContext.UserMessage.Text)));

            responses.Should().NotContainNulls();
        }

        [Test]
        [Timeout(10_000)] // 10 seconds
        [InlineAutoData(Secrets.GroupChatIdForBot)]
        public async Task ShouldSendMessageWithCancelCallbackButton(
            long chatId,
            string messageText)
        {
            var fixture = AutoFixtureFactory.Create();
            fixture.Freeze<ChatModel>(b => b.With(c => c.Id, chatId));
            var messageContext = fixture.Build<MessageContext>()
                .Create();
            using var container = CreateContainer(messageContext);
            var callbackFactory = container.Create<CallbackFactory>();
            var button = fixture.Build<InlineButton>()
                .With(b => b.CallbackData, () => callbackFactory.CreateDataForCancellation())
                .Create();
            var sut = container.Create<TgClientWrapper>();

            var response = await sut.SendMessageAsync(
                messageText,
                new InlineButtonCollection(button));

            response.Should().NotBeNull();
        }

        private static AutoMock CreateContainer(
            MessageContext messageContext,
            Action<ContainerBuilder>? beforeBuild = null) =>
            AutoMockContainerFactory.Create(
                builder =>
                {
                    builder.RegisterOptions(
                        new BotOptions { Token = Secrets.BotToken });

                    builder.RegisterModules(new TelegramBotModule());
                    builder.RegisterInstance(messageContext);

                    beforeBuild?.Invoke(builder);
                });
    }
}
