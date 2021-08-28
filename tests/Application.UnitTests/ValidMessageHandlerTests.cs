using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.UnitTests.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    public class ValidMessageHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSendLoadingStartedWithCancellation(
            MessageValidEvent @event,
            MessageModel loadingProcessMessage)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create();
            var sut = container.Create<ValidMessageHandler>();
            var lazyCapture = new LazyCapture<InlineButtonCollection>();
            container.Mock<ITgClient>()
                .Setup(
                    c => c.SendMessageAsync(
                        @event.Aggregate.ChatId,
                        "Loading started.",
                        Capture.With(lazyCapture.Match),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadingProcessMessage)
                .Verifiable();

            await sut.Handle(@event);

            lazyCapture.Value.VerifyCancelButton(@event);
            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            uncommittedEvents.Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<LoadingProcessMessageSentEvent>()
                .Which.MessageId.Should()
                .Be(loadingProcessMessage.Id);
            container.VerifyMessageSaved(@event.Aggregate);
        }


    }
}
