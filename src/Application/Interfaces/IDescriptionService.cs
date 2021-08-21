using YoutubeMusicBot.Application.Interfaces.Wrappers;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IDescriptionService
    {
        IFileInfo? GetFileOrNull(IFileInfo file);
    }
}
