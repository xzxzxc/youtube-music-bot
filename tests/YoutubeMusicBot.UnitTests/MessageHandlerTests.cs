using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using YoutubeMusicBot.DependencyInjection;
using YoutubeMusicBot.Handlers;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Tests;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.UnitTests
{
    public class MessageHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly MessageModel _validMessage;

        public MessageHandlerTests()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            _fixture = AutoFixtureFactory.Create();
            _validMessage = _fixture
                .Build<MessageModel>()
                .With(m => m.Text, "https://youtu.be/wuROIJ0tRPU")
                .Create();
        }

        [Test]
        [TestCase("", "Message must be not empty.")]
        [TestCase("kljjk", "Message must be valid URL.")]
        [TestCase("htt://test.com", "Message must be valid URL.")]
        [TestCase("http:/test.com", "Message must be valid URL.")]
        [TestCase("test.com", "Message must be valid URL.")]
        public async Task ShouldSendErrorMessageOnInvalidUrl(
            string url,
            string expectedMessage)
        {
            using var container = CreateAutoMockContainer();
            var handler = container.Create<MessageHandler>();
            var message = _fixture
                .Build<MessageModel>()
                .With(m => m.Text, url)
                .Create();

            await handler.Handle(new MessageHandler.Request(message));

            container.Mock<ITgClientWrapper>()
                .Verify(
                    c => c.SendMessageAsync(
                        expectedMessage,
                        It.IsAny<InlineButtonCollection?>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public async Task ShouldSendLoadingStarted()
        {
            using var container = CreateAutoMockContainer();
            var handler = container.Create<MessageHandler>();

            await handler.Handle(new MessageHandler.Request(_validMessage));

            container.Mock<ITgClientWrapper>()
                .Verify(
                    c => c.SendMessageAsync(
                        "Loading started.",
                        It.IsAny<InlineButtonCollection?>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldSendCancelButton(string callbackData)
        {
            using var container = CreateAutoMockContainer(
                b => b.RegisterInstance(
                    Mock.Of<ICancellationRegistration>(
                        f => f.RegisterNewProvider(It.IsAny<CancellationToken>())
                            == Mock.Of<ICancellationProvider>(
                                p =>
                                    p.CallbackData == callbackData))));
            var handler = container.Create<MessageHandler>();

            await handler.Handle(new MessageHandler.Request(_validMessage));

            container.Mock<ITgClientWrapper>()
                .Verify(
                    c => c.SendMessageAsync(
                        It.IsAny<string>(),
                        It.Is<InlineButtonCollection?>(
                            c => c != null
                                && c.First().Text == "Cancel"
                                && c.First().CallbackData == callbackData),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldCallYoutubeClientWithCancellation(
            CancellationToken cancellationToken)
        {
            using var container = CreateAutoMockContainer(
                b => b.RegisterInstance(
                    Mock.Of<ICancellationRegistration>(
                        f => f.RegisterNewProvider(It.IsAny<CancellationToken>())
                            == Mock.Of<ICancellationProvider>(
                                p => p.Token == cancellationToken))));
            var handler = container.Create<MessageHandler>();

            await handler.Handle(new MessageHandler.Request(_validMessage));

            container.Mock<IYoutubeDlWrapper>()
                .Verify(
                    c => c.DownloadAsync(
                        _validMessage.Text,
                        cancellationToken),
                    Times.Once);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldSetReplyMessageToContext(MessageModel replyMessage)
        {
            var messageContext = new MessageContext(_validMessage);
            var scopeFactoryMock = new Mock<IMessageScopeFactory>();
            using var container = CreateAutoMockContainer(b => b.RegisterMock(scopeFactoryMock));
            scopeFactoryMock
                .Setup(s => s.Create(_validMessage))
                .Returns(
                    container.Container.BeginLifetimeScope(
                        b => b.RegisterInstance(messageContext)));
            var handler = container.Create<MessageHandler>();
            container.Mock<ITgClientWrapper>()
                .Setup(
                    w => w.SendMessageAsync(
                        It.IsAny<string>(),
                        It.IsAny<InlineButtonCollection?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(replyMessage);

            await handler.Handle(new MessageHandler.Request(_validMessage));

            messageContext.MessageToUpdate.Should().BeSameAs(replyMessage);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldDeleteReplyMessage(MessageModel replyMessage)
        {
            using var container = CreateAutoMockContainer();
            var handler = container.Create<MessageHandler>();
            container.Mock<ITgClientWrapper>()
                .Setup(
                    w => w.SendMessageAsync(
                        It.IsAny<string>(),
                        It.IsAny<InlineButtonCollection?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(replyMessage);

            await handler.Handle(new MessageHandler.Request(_validMessage));

            container.Mock<ITgClientWrapper>()
                .Verify(w => w.DeleteMessageAsync(replyMessage.Id, It.IsAny<CancellationToken>()));
        }

        private AutoMock CreateAutoMockContainer(Action<ContainerBuilder>? beforeBuild = null)
        {
            var mockRepository = new MockRepository(MockBehavior.Loose)
            {
                DefaultValueProvider = new AutoFixtureValueProvider(_fixture),
            };
            return AutoMock.GetFromRepository(
                mockRepository,
                builder =>
                {
                    builder.RegisterModule(new CommonModule());
                    builder.RegisterModule(new MessageHandlerModule());
                    builder.RegisterMock(mockRepository.Create<ITgClientWrapper>());
                    builder.RegisterMock(mockRepository.Create<IYoutubeDlWrapper>());
                    beforeBuild?.Invoke(builder);
                });
        }
    }
}
