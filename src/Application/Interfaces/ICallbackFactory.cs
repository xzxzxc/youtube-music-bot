using YoutubeMusicBot.Domain.Enums;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface ICallbackFactory
    {
        CallbackAction GetActionFromData(string callbackData);

        string CreateDataForCancellation();
    }
}
