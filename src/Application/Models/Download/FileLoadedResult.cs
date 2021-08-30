using YoutubeMusicBot.Application.Abstractions.Download;

namespace YoutubeMusicBot.Application.Models.Download
{
    public record FileLoadedResult(
        string MusicFilePath,
        string? DescriptionFilePath) :
        IDownloadResult;
}
