using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;

namespace YoutubeMusicBot.UnitTests
{
    public class ValidMessageHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendLoadingStartedWithCancellation(
            MessageValidEvent @event)
        {
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<ValidMessageHandler>();

            await sut.Handle(@event);

            InlineButtonCollection? buttons = null;
            var match = new CaptureMatch<InlineButtonCollection>(c => buttons = c);
            container.Mock<ITgClient>()
                .Verify(
                    c => c.SendMessageAsync(
                        @event.Aggregate.ChatId,
                        "Loading started.",
                        Capture.With(match),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            buttons.Should().NotBeNull();
            var button = buttons.Should().ContainSingle().Which;
            button.Text.Should().Be("Cancel");
            button.CallbackData.Should().Be(@event.GetCancellationId());
        }
    }
}
