using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.CommandHandlers;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;
using YoutubeMusicBot.IntegrationTests.Common.Moq;

namespace YoutubeMusicBot.Console.UnitTests
{
    [Parallelizable]
    public class BotUpdatesProcessorTests
    {
        [Test]
        public async Task ShouldSendMessageRequest()
        {
            var cancellationToken = new CancellationToken();
            var messageUpdate = AutoFixtureFactory.Create().Build<Update>()
                .OmitAutoProperties()
                .With(c => c.Id)
                .With(c => c.Message)
                .Create();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<ITelegramBotClient>()
                .Setup(
                    c => c.GetUpdatesAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<IEnumerable<UpdateType>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { messageUpdate });
            var sut = container.Create<BotUpdatesProcessor>();

            await sut.ProcessUpdatesAsync(cancellationToken);

            var lazyCapture = new LazyCapture<MessageHandler.Command>();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(Capture.With(lazyCapture.Match), cancellationToken),
                    Times.Once);
            lazyCapture.Value.MessageId.Should().Be(messageUpdate.Message.MessageId);
            lazyCapture.Value.Text.Should().Be(messageUpdate.Message.Text);
            lazyCapture.Value.ChatId.Should().Be(messageUpdate.Message.Chat.Id);
        }

        [Test]
        public async Task ShouldSendCallbackRequestIfEaArchitectureDisabled()
        {
            var cancellationToken = new CancellationToken();
            var callbackUpdate = AutoFixtureFactory.Create().Build<Update>()
                .OmitAutoProperties()
                .With(c => c.Id)
                .With(c => c.CallbackQuery)
                .Create();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<ITelegramBotClient>()
                .Setup(
                    c => c.GetUpdatesAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<IEnumerable<UpdateType>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { callbackUpdate });
            var sut = container.Create<BotUpdatesProcessor>();

            await sut.ProcessUpdatesAsync(cancellationToken);

            var lazyCapture = new LazyCapture<CallbackQueryHandler.Command>();
            container.Mock<IMediator>()
                .Verify(
                    s => s.Send(Capture.With(lazyCapture.Match), It.IsAny<CancellationToken>()),
                    Times.Once);
            lazyCapture.Value.Should().NotBeNull();
            lazyCapture.Value.CallbackData.Should().Be(callbackUpdate.CallbackQuery.Data);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCatchAndLogException()
        {
            // TODO: add test
        }
    }
}
