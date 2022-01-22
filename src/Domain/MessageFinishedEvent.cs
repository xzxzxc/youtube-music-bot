using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record MessageFinishedEvent : EventBase<Message>;

    public record MessageCancelledEvent : EventBase<Message>;
}
