using System.Net;

namespace BlueSeaGameFramework.Network.Buffer
{
    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        ACK = 0,        // 确认报文（用于可靠传输确认）
        Login = 1,      // 登录业务逻辑报文
        CONNECT = 2,    // 连接请求报文
        DISCONNECT = 3, // 断开连接报文
    }

    /// <summary>
    /// 缓冲区工厂类（用于创建各种类型的网络数据包）
    /// </summary>
    public class BufferFactory
    {
        /// <summary>
        /// 创建ACK确认包
        /// </summary>
        /// <param name="bufferEntity">需要确认的原始数据包</param>
        /// <returns>ACK确认包实例</returns>
        public static BufferEntity creatAckBuffer(BufferEntity bufferEntity)
        {
            // 基于原始包创建ACK包（通常只需复制关键字段）
            BufferEntity AckbufferEntity = new BufferEntity(bufferEntity);
            return AckbufferEntity;
        }

        /// <summary>
        /// 从网络接收数据创建缓冲区实体
        /// </summary>
        /// <param name="resultRemoteEndPoint">数据来源终结点</param>
        /// <param name="resultBuffer">原始网络字节数据</param>
        /// <returns>解析后的缓冲区实体</returns>
        public static BufferEntity creatEntityByResult(IPEndPoint resultRemoteEndPoint, byte[] resultBuffer)
        {
            // 从网络字节数据解析出缓冲区实体
            BufferEntity bufferEntity = BufferEntity.ParseFromNetworkPacket(resultBuffer);
            // 设置数据来源地址
            bufferEntity.OriginEndpoint = resultRemoteEndPoint;
            return bufferEntity;
        }

        /// <summary>
        /// 创建连接请求包
        /// </summary>
        /// <param name="targetEndPoint">目标服务器地址</param>
        /// <param name="sendSn">发送序列号</param>
        /// <returns>连接请求包实例</returns>
        public static BufferEntity creatConnectEntity(IPEndPoint targetEndPoint, int sendSn)
        {
            // 创建包含目标地址和序列号的基础连接包
            BufferEntity bufferEntity = new BufferEntity(targetEndPoint, sendSn);
            return bufferEntity;
        }

        /// <summary>
        /// 创建业务数据发送包（带协议缓冲区数据）
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <param name="iMessage">协议消息体</param>
        /// <param name="targetEndPoint">目标地址</param>
        /// <param name="sendSn">发送序列号</param>
        /// <param name="sessionId">会话ID</param>
        /// <returns>完整的数据发送包</returns>
        public static BufferEntity creatEntityForSend(MessageId msgId, IMessage iMessage, IPEndPoint targetEndPoint, int sendSn, int sessionId)
        {
            // 将协议消息序列化为字节数组
            byte[] protocolData = ProtobufHelper.ToBytes(iMessage);
            
            // 创建完整的数据包（包含会话信息、序列号、消息类型等）
            BufferEntity bufferEntity = new BufferEntity(
                sessionId, 
                sendSn, 
                0, // 假设0表示默认状态
                MessageType.Login, 
                msgId, 
                protocolData);
            
            // 设置目标地址
            bufferEntity.TargetEndpoint = targetEndPoint;
            return bufferEntity;
        }
    }
}