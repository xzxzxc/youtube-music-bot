using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record LoadingProcessMessageSentEvent(int MessageId) : EventBase<Message>;
}
