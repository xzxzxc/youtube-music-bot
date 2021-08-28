using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;

namespace YoutubeMusicBot.Application.Models.YoutubeDownloader
{
    public record FileLoadedResult(
        string MusicFilePath,
        string? DescriptionFilePath) :
        IDownloadResult;
}
