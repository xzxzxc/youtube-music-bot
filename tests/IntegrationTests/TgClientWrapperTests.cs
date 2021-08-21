using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.ResolveAnything;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Infrastructure.Wrappers;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace YoutubeMusicBot.IntegrationTests
{
    [Parallelizable]
    public class TgClientWrapperTests : IDisposable
    {
        private readonly IHost _host;

        public TgClientWrapperTests()
        {
            _host = Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, builder) => builder.RegisterOptions(
                            new BotOptions { Token = Secrets.BotToken })
                        .RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource()))
                .Build();
        }

        [Test]
        [Timeout(120_000)] // 2 minutes
        [InlineAutoData(Secrets.GroupChatId, 21)]
        public async Task ShouldHandleFloodControl(
            long chatId,
            int messagesCount,
            string messageText)
        {
            var fixture = AutoFixtureFactory.Create();
            var chat = fixture
                .Build<ChatModel>()
                .With(c => c.Id, chatId)
                .Create();
            var message = fixture
                .Build<MessageModel>()
                .With(m => m.Chat, chat)
                .Create();
            await using var scope = _host.Services.GetRequiredService<ILifetimeScope>().BeginLifetimeScope(
                c => c.RegisterInstance(new MessageContext(message)));

            var wrapper = scope.Resolve<TgClientWrapper>();

            var responses = await Task.WhenAll(
                Enumerable.Range(0, messagesCount)
                    .Select(_ => wrapper.SendMessageAsync(messageText)));

            responses.Should().NotContainNulls();
        }

        [Test]
        [Timeout(10_000)] // 10 seconds
        [InlineAutoData(Secrets.GroupChatId)]
        public async Task ShouldSendMessageWithCancelCallbackButton(
            long chatId,
            string messageText)
        {
            var fixture = AutoFixtureFactory.Create();
            var chat = fixture
                .Build<ChatModel>()
                .With(c => c.Id, chatId)
                .Create();
            var message = fixture
                .Build<MessageModel>()
                .With(m => m.Chat, chat)
                .Create();
            await using var scope = _host.Services.GetRequiredService<ILifetimeScope>().BeginLifetimeScope(
                c => c.RegisterInstance(new MessageContext(message)));

            var callbackFactory = scope.Resolve<CallbackFactory>();
            var wrapper = scope.Resolve<TgClientWrapper>();
            var button = fixture.Build<InlineButton>()
                .With(b => b.CallbackData, () => callbackFactory.CreateDataForCancellation())
                .Create();

            var response = await wrapper.SendMessageAsync(messageText, button);

            response.Should().NotBeNull();
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
