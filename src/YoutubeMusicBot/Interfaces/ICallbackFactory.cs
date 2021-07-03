namespace YoutubeMusicBot.Interfaces
{
    public interface ICallbackFactory
    {
        CallbackAction GetActionFromData(byte[] callbackData);

        byte[] CreateDataForCancellation();
    }
}
