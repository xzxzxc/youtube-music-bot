using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Abstractions.Telegram;
using YoutubeMusicBot.Application.DependencyInjection;
using YoutubeMusicBot.Application.Models.Telegram;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.Infrastructure.IntegrationTest.Helpers;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;

namespace YoutubeMusicBot.Infrastructure.IntegrationTest
{
    [Parallelizable]
    public class TgClientTests
    {
        [Test]
        [Timeout(120_000)] // 2 minutes
        [InlineAutoData(Secrets.GroupChatIdForBot, 21)]
        public async Task ShouldHandleFloodControl(
            long chatId,
            int messagesCount,
            string messageText)
        {
            using var container = await CreateContainer();
            var sut = container.Create<TgClient>();

            var responses = await Task.WhenAll(
                Enumerable.Range(0, messagesCount)
                    .Select(_ => sut.SendMessageAsync(chatId, messageText)));

            responses.Should().NotContainNulls();
        }

        [Test]
        [Timeout(10_000)] // 10 seconds
        [InlineAutoData(Secrets.GroupChatIdForBot)]
        public async Task ShouldSendMessageWithCancelCallbackButton(
            long chatId,
            SimpleTestEvent @event,
            string messageText)
        {
            var fixture = AutoFixtureFactory.Create();
            using var container = await CreateContainer(
                b => b.RegisterModule(
                    new CallbackDataModule(Assembly.GetExecutingAssembly())));
            var callbackFactory = container.Container.Resolve<ICallbackDataFactory>();
            var button = fixture.Build<InlineButton>()
                .With(b => b.CallbackData, () => callbackFactory.CreateForCancel(@event))
                .Create();
            var sut = container.Create<TgClient>();

            var response = await sut.SendMessageAsync(
                chatId,
                messageText,
                new InlineButtonCollection(button));

            response.Should().NotBe(default);
        }

        private static ValueTask<AutoMock> CreateContainer(
            Action<ContainerBuilder>? beforeBuild = null) =>
            AutoMockInfrastructureContainerFactory.Create(
                builder =>
                {
                    builder.RegisterOptions(
                        new BotOptions { Token = Secrets.BotToken });

                    builder.RegisterModules(new TelegramBotModule());

                    beforeBuild?.Invoke(builder);
                });

        public class TestAggregate : AggregateBase<TestAggregate>
        {
        }

        public record SimpleTestEvent : EventBase<TestAggregate>;
    }
}
