using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;

namespace YoutubeMusicBot.Application.Models.YoutubeDownloader
{
    public record FileLoadedResult(IFileInfo Value) : IDownloadResult;
}
