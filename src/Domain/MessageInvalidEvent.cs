using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record MessageInvalidEvent(string ValidationMessage) : EventBase<Message>;
}
