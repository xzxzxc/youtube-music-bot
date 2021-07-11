using YoutubeMusicBot.Enums;

namespace YoutubeMusicBot.Interfaces
{
    public interface ICallbackFactory
    {
        CallbackAction GetActionFromData(string callbackData);

        string CreateDataForCancellation();
    }
}
