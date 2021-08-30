using YoutubeMusicBot.Application.Abstractions.Download;

namespace YoutubeMusicBot.Application.Models.Download
{
    public record RawTitleParsedResult(string Value) : IDownloadResult;

}
