using FluentAssertions;
using YoutubeMusicBot.Application.Models.Telegram;

namespace YoutubeMusicBot.Application.UnitTests.Extensions
{
    public static class InlineButtonCollectionExtensions
    {
        public static void VerifyCancelButton(
            this InlineButtonCollection buttons,
            string callbackData)
        {
            var button = buttons.Should().ContainSingle().Which;
            button.Text.Should().Be("Cancel");
            button.CallbackData.Should().Be(callbackData);
        }
    }
}
