using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public record NewMusicFileEvent(string FullPath) : EventBase<Message>;
}
