using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record FileToBeSentCreatedEvent(string FilePath, string Title) : EventBase<Message>;
}
