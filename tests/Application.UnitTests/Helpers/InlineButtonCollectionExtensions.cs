using FluentAssertions;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.UnitTests.Helpers
{
    public static class InlineButtonCollectionExtensions
    {
        public static void VerifyCancelButton<TAggregate>(
            this InlineButtonCollection buttons,
            EventBase<TAggregate> @event)
            where TAggregate : AggregateBase<TAggregate>
        {
            var button = buttons.Should().ContainSingle().Which;
            button.Text.Should().Be("Cancel");
            button.CallbackData.Should().Be(@event.GetCancellationId());
        }
    }
}
