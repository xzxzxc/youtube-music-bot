using YoutubeMusicBot.Console.Enums;

namespace YoutubeMusicBot.Console.Interfaces
{
    public interface ICallbackFactory
    {
        CallbackAction GetActionFromData(string callbackData);

        string CreateDataForCancellation();
    }
}
