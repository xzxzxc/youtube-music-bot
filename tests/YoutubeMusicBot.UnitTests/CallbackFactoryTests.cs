using System.Text;
using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Services;

namespace YoutubeMusicBot.UnitTests
{
    public class CallbackFactoryTests
    {
        private const int TelegramMaxCallbackDataSize = 64;

        [Test]
        public void ShouldCreateValidData()
        {
            var factory = new CallbackFactory();

            var callbackData = factory.CreateDataForCancellation();

            callbackData.Should().NotBeNullOrEmpty();
            Encoding.Unicode.GetBytes(callbackData!)
                .Should()
                .HaveCountLessOrEqualTo(TelegramMaxCallbackDataSize);
        }
    }
}
