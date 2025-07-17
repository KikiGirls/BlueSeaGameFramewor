namespace BlueSeaGameFramework.server.Network.Message;


public static class MessageIdHelper
{
    // 根据 ID 获取对应的枚举值，如果无效则返回 None
    public static MessageId FromId(int id)
    {
        if (Enum.IsDefined(typeof(MessageId), id))
        {
            return (MessageId)id;
      
        }
        else
        {
            return MessageId.None;  // 返回特定的无效值
        }
    }
}

public enum MessageId : int
{
    None = 0,
    LoginRequest = 1001,
    LogoutRequest = 1002,
    ChatMessage = 1003,
}