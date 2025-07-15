
namespace BlueSeaGameFramework.Network.Client;

public class NetEvent : Singleton<NetEvent>
{
    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void AddEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage, new()
    {
        throw new NotImplementedException();
    }

    public void RemoveEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage
    {
        throw new NotImplementedException();
    }
}