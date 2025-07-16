using System.Net;

namespace BlueSeaGameFramework.Network.Buffer
{
    public enum MessageType
    {
        ACK = 0, //确认报文
        Login = 1, //业务逻辑的报文
        CONNECT = 2,
        DISCONNECT = 3,
    }
    public class BufferFactory

    {
        public static BufferEntity creatEntity(IPEndPoint resultRemoteEndPoint, byte[] resultBuffer)
        {
            throw new NotImplementedException();
        }

        public static BufferEntity creatEntity(MessageId resultRemoteEndPoint, IMessage resultBuffer, int sendSn)
        {
            throw new NotImplementedException();
        }

        public static BufferEntity creatAckBuffer(BufferEntity C)
        {
            throw new NotImplementedException();
        }
    }
}

