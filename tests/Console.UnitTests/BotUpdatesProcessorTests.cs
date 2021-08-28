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
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Tests.Common;

namespace Console.UnitTests
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

            MessageHandler.Request? sentRequest = null;
            var match = new CaptureMatch<MessageHandler.Request>(c => sentRequest = c);
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send(Capture.With(match), cancellationToken),
                    Times.Once);
            sentRequest.Should().NotBeNull();
            sentRequest!.Value.Should().NotBeNull();
            sentRequest.Value.Id.Should().Be(messageUpdate.Message.MessageId);
            sentRequest.Value.Text.Should().Be(messageUpdate.Message.Text);
            sentRequest.Value.Chat.Should().NotBeNull();
            sentRequest.Value.Chat.Id.Should().Be(messageUpdate.Message.Chat.Id);
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

            var capture = new LazyCapture<CallbackQueryHandler.Request>();
            container.Mock<IMediator>()
                .Verify(
                    s => s.Send(Capture.With(capture.Match), It.IsAny<CancellationToken>()),
                    Times.Once);
            var sentRequest = capture.Value;
            sentRequest.Value.Should().NotBeNull();
            sentRequest.Value.CallbackData.Should().Be(callbackUpdate.CallbackQuery.Data);
            sentRequest.Value.Chat.Should().NotBeNull();
            sentRequest.Value.Chat.Id.Should().Be(callbackUpdate.CallbackQuery.Message.Chat.Id);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCatchAndLogException()
        {
            // TODO: add test
        }
    }
}
