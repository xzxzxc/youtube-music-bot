using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Application.Validation;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;

namespace YoutubeMusicBot.Application.UnitTests.Services
{
    public class MessageValidatorTests : BaseParallelizableTest
    {
        [Test]
        [TestCase("http://test.com")]
        public async Task ShouldReturnValidResultOnValidUrl(string url)
        {
            var fixture = AutoFixtureFactory.Create();
            var message = fixture.Build<Message>()
                .FromFactory(
                    (int externalId, long chatId) => new Message(externalId, text: url, chatId))
                .Create();
            var sut = new MessageValidator();

            var res = await sut.ValidateAsync(message);

            res.IsValid.Should().BeTrue();
        }

        [Test]
        [TestCase("", "Message must be not empty.")]
        [TestCase("kljjk", "Message must be valid URL.")]
        [TestCase("htt://test.com", "Message must be valid URL.")]
        [TestCase("http:/test.com", "Message must be valid URL.")]
        [TestCase("test.com", "Message must be valid URL.")]
        public async Task ShouldReturnFailedResultOnInvalidUrl(
            string url,
            string expectedMessage)
        {
            var fixture = AutoFixtureFactory.Create();
            var message = fixture.Build<Message>()
                .FromFactory(
                    (int externalId, long chatId) => new Message(externalId, text: url, chatId))
                .Create();
            var sut = new MessageValidator();

            var res = await sut.ValidateAsync(message);

            res.IsValid.Should().BeFalse();
            res.Errors.Should()
                .ContainSingle()
                .Which.ErrorMessage.Should()
                .Be(expectedMessage);
        }
    }
}
