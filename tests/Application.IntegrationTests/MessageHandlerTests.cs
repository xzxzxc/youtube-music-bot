using AutoFixture;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Models;
using TagFile = TagLib.File;
using static Application.IntegrationTests.CommonFixture;

namespace Application.IntegrationTests
{
    [Parallelizable]
    public class MessageHandlerTests
    {
        private readonly MessageModel _validMessage;

        public MessageHandlerTests()
        {
            _validMessage = FixtureInstance
                .Build<MessageModel>()
                .With(m => m.Text, "https://youtu.be/wuROIJ0tRPU")
                .Create();
        }

        [Test]
        public async Task ShouldCreateMessageAggregate()
        {
            var handler = Container.Create<MessageHandler>();
            var dbContext = Container.Create<IDbContext>();

            await handler.Handle(new MessageHandler.Request(_validMessage));

            var message = await dbContext.Messages
                .FirstOrDefaultAsync(m => m.ExternalId == _validMessage.Id);
            message.Should().NotBeNull();
        }
    }
}
