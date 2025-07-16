using System.Collections.Concurrent;
using System.Net;
using BlueSeaGameFramework.Network.Transport;


namespace BlueSeaGameFramework.Network.Client;

public class UClient
{
    
    public IPEndPoint endPoint;
    UdpTransport uSocket;
    public int sessionID;
    public int sendSN = 0;
    public int handleSN = 0;
    private ConcurrentDictionary<int, BufferEntity> waitHandle;

    public UClient()
    {
        uSocket = new UdpTransport();
    }
    public void Connect(string ip, int port)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Send(MessageId msgId, IMessage iMessage)
    {
        sendSN += 1;
        uSocket.Send(msgId, iMessage, sendSN); 
    }
    
    

    public void Tick()
    {
        throw new NotImplementedException();
    }

    public void HandleLogicPackage(BufferEntity bufferEntity)
    {
        if (bufferEntity.SequenceNumber <= handleSN)
        {
            return;
        }

        if (bufferEntity.SequenceNumber > handleSN + 1)
        {
            if (waitHandle.TryAdd(bufferEntity.SequenceNumber, bufferEntity))
            {
                Console.WriteLine("收到错序的报文");
            }
            return;
        }
        
        handleSN =  bufferEntity.SequenceNumber;
        Dispatch(bufferEntity);

        BufferEntity nextBuffer;
        if (waitHandle.TryRemove(handleSN+1,out nextBuffer))
        {
            //这里是判断缓冲区有没有存在下一条数据
            HandleLogicPackage(nextBuffer);
        }
        
    }

    private void Dispatch(BufferEntity bufferEntity)
    {
        MessageId messageId = bufferEntity.MessageId;
        var messageWrapper =  new MessageWrapper<BufferEntity>(bufferEntity);
        NetworkManager.netEvent.Dispatch(messageId, messageWrapper);
    }
    
}