


namespace BlueSeaGameFramework.Network.Transport
{
    public class UdpTransport
    {
        UdpClient  udpClient;
        IPEndPoint endPoint;
        static UClient uClient = NetworkManager.client;
        
        ConcurrentQueue<UdpReceiveResult> awaitHandle;
        //缓存已经发送的报文
        ConcurrentDictionary<int, BufferEntity> sendPackage;

        public UdpTransport()
        {
            udpClient = new UdpClient(0);
            this.endPoint = uClient.endPoint;
            awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
            sendPackage = new ConcurrentDictionary<int, BufferEntity>();
            AsyncReceiveTask();
            CheckOutTime();
        }

        TimeSpan overtime = TimeSpan.FromMilliseconds(150);
        private async void CheckOutTime()
        {
            await Task.Delay(overtime);
            foreach (var package in sendPackage.Values)
            {
                //确定是不是超过最大发送次数  关闭socket
                if (package.RetryCount >= 10)
                {
                    Console.WriteLine($"重发次数超过10次,关闭socket");
                    NetworkManager.client.Dispose();
                    return;
                }
               
                //150
                if (DateTime.Now-package.SendTime>=(package.RetryCount+1)*overtime)
                {
                    package.RetryCount += 1;
                    Console.WriteLine($"超时重发,次数:{package.RetryCount}");
                    SendSafeAsync(package.BufferData, endPoint);
                }
            }
            CheckOutTime();
        }

        public async void AsyncReceiveTask()
        {
            while (udpClient != null && udpClient.Available > 0)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    awaitHandle.Enqueue(result);
                    Console.WriteLine("接受到了消息");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public async Task<bool> SendSafeAsync(byte[] data, IPEndPoint endPoint)
        {
            if (udpClient == null || data == null || data.Length == 0)
                return false;

            try
            {
                int sentBytes = await udpClient.SendAsync(data, data.Length, endPoint);
                return sentBytes == data.Length; // 验证是否全部发送
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // 目标不可达（ICMP错误）
                return false;
            }
            catch (ObjectDisposedException)
            {
                // Socket已关闭
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP发送失败: {ex.Message}");
                return false;
            }
        }

        public void Handle()
        {
            if (awaitHandle.TryDequeue(out UdpReceiveResult result))
            {
                try
                {
                    BufferEntity bufferEntity = BufferFactory.creatEntity(result.RemoteEndPoint, result.Buffer);
                    Console.WriteLine("处理消息");
                    switch (bufferEntity.MessageType)
                    {
                        case MessageType.ACK:
                            HandleAckPacket(bufferEntity);
                            break; // 必须明确终止
    
                        case MessageType.Login:
                            uClient.HandleLogicPackage(bufferEntity);
                            SendAckPacket(bufferEntity);
                            break;
                        case MessageType.CONNECT:
                            HandleConnect(bufferEntity);
                            break;
                        default:
                            LogUnknownMessageType(bufferEntity.MessageType);
                            break;
                    }
                    
                }
                catch (InvalidDataException ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        private void HandleConnect(BufferEntity bufferEntity)
        {
            throw new NotImplementedException();
        }

        private void SendAckPacket(BufferEntity bufferEntity)
        {
            BufferEntity ackBuffer = BufferFactory.creatAckBuffer(bufferEntity);
            SendSafeAsync(ackBuffer.BufferData, endPoint);
        }
        private void HandleAckPacket(BufferEntity bufferEntity)
        {
            
            if (sendPackage.TryRemove(bufferEntity.SequenceNumber,out bufferEntity))
            {
                Console.WriteLine($"收到ACK确认报文,序号是:{bufferEntity.SequenceNumber}");
            }
        }
        

        private void LogUnknownMessageType(MessageType bufferEntityMessageType)
        {
            Console.WriteLine("收到未知消息");
        }


        public void Close()
        {
            if (udpClient != null) udpClient.Close();
        }

        public void Send(MessageId msgId, IMessage iMessage, int sendSN)
        {
            BufferEntity bufferEntity = BufferFactory.creatEntity(msgId, iMessage, sendSN);
            SendSafeAsync(bufferEntity.BufferData, endPoint);
        }
    }
}