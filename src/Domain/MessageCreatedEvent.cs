using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record MessageCreatedEvent(
            int ExternalId,
            string Text,
            long ChatId)
        : EventBase<Message>;
}
