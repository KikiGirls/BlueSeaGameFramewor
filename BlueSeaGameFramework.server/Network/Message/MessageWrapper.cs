namespace BlueSeaGameFramework.server.Network.Message;

/// <summary>
/// 网络消息包装器，包含业务消息及相关元信息（如时间戳、会话ID等）
/// </summary>
public class MessageWrapper<T> 
{
    public IMessage Message { get; }
    public MessageWrapper(BufferEntity bufferEntity)
    {
        byte[] byteMessage = bufferEntity.ProtocolData;
    }
}