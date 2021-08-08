using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using YoutubeMusicBot.Console.Handlers;

namespace YoutubeMusicBot.UnitTests
{
    public class UpdateHandlerTests
    {
        [Test]
        [AutoData]
        public async Task ShouldSendMessageRequestIfTextUpdate(
            long chatId,
            int messageId,
            string text)
        {
            var mediatorMock = new Mock<IMediator>();
            var handler = new UpdateHandler(mediatorMock.Object);

            await handler.Handle(
                new UpdateHandler.Request(
                    new Update
                    {
                        Message = new Message
                        {
                            Text = text,
                            Chat = new Chat { Id = chatId, },
                            MessageId = messageId,
                        },
                    }));

            mediatorMock.Verify(
                s => s.Send(
                    It.Is<MessageHandler.Request>(
                        r => r.Value.Text == text
                            && r.Value.Chat.Id == chatId
                            && r.Value.Id == messageId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        [AutoData]
        public async Task ShouldSendCallbackRequestIfCallbackUpdate(
            long chatId,
            string callbackData)
        {
            var mediatorMock = new Mock<IMediator>();
            var handler = new UpdateHandler(mediatorMock.Object);

            await handler.Handle(
                new UpdateHandler.Request(
                    new Update
                    {
                        CallbackQuery = new CallbackQuery
                        {
                            Data = callbackData,
                            Message = new Message
                            {
                                Chat = new Chat { Id = chatId, },
                            },
                        },
                    }));

            mediatorMock.Verify(
                s => s.Send(
                    It.Is<CallbackQueryHandler.Request>(
                        r => r.CallbackQuery.CallbackData == callbackData
                            && r.CallbackQuery.Chat.Id == chatId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
