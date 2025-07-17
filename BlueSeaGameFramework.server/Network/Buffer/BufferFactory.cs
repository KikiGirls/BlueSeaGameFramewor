using System.Threading.Tasks.Dataflow;
using BlueSeaGameFramework.server.Utils;

namespace BlueSeaGameFramework.Server.Network.Buffer
{
    public class BufferFactory
    {
        public static BufferEntity createAckPackage(BufferEntity package)
        {
            return new BufferEntity(package);
        }

        public static BufferEntity createLogicPackage(UClient uClient, MessageId messageId, IMessage message)
        {
            BufferEntity bufferEntity = new BufferEntity(uClient.endPoint,uClient.SessionId,0,0, MessageType.Logic,
                messageId,ProtobufHelper.ToBytes(message));
            return bufferEntity;
        }
     }
}