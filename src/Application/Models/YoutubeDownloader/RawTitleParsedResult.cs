using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.YoutubeDownloader;

namespace YoutubeMusicBot.Application.Models.YoutubeDownloader
{
    public record RawTitleParsedResult(string Value) : IDownloadResult;

}
