using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record MusicFileCreatedEvent(string MusicFilePath, string? DescriptionFilePath) :
        EventBase<Message>;
}
