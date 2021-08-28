using System;
using System.Threading;
using Autofac.Extras.Moq;
using Moq;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.UnitTests.Extensions
{
    public static class AutoMockExtensions
    {
        public static void VerifyMessageSaved(
            this AutoMock container,
            Message message,
            Times? times = null) =>
            container.Mock<IRepository<Message>>()
                .Verify(
                    r => r.SaveAndEmitEventsAsync(message, It.IsAny<CancellationToken>()),
                    times ?? Times.Once());
    }
}
