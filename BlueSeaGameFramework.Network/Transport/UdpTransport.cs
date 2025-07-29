using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace BlueSeaGameFramework.Network.Transport
{
    /// <summary>
    /// UDP传输层实现类，负责UDP通信的核心功能
    /// </summary>
    public class UdpTransport
    {
        UdpClient udpClient;                      // UDP客户端实例
        
        ConcurrentQueue<UdpReceiveResult> awaitHandle;  // 待处理消息队列
        // 缓存已发送的报文（用于超时重传）
        ConcurrentDictionary<int, BufferEntity> sendPackage;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="MyPort">本地绑定端口</param>
        /// <param name="serverEndPoint">服务器终结点</param>
        public UdpTransport(int MyPort)
        {
            udpClient = new UdpClient(MyPort);
            awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
            sendPackage = new ConcurrentDictionary<int, BufferEntity>();
            AsyncReceiveTask(); // 启动异步接收任务
            CheckOutTime(); // 启动超时检测任务
            Task.Run(Handle, ct.Token);
        }

        CancellationTokenSource ct = new CancellationTokenSource();
        

        TimeSpan overtime = TimeSpan.FromMilliseconds(150);  // 超时时间设定为150ms
        
        /// <summary>
        /// 超时检测方法（循环执行）
        /// </summary>
        private async void CheckOutTime()
        {
            await Task.Delay(overtime);
            foreach (var package in sendPackage.Values)
            {
                // 检查是否超过最大重试次数（10次）
                if (package.RetryCount >= 10)
                {
                    Console.WriteLine($"重发次数超过10次,关闭socket");
                    NetworkManager.Instance.client.Dispose();
                    return;
                }
               
                // 动态计算超时时间：(重试次数+1)*基础超时时间
                if (DateTime.Now - package.SendTime >= (package.RetryCount + 1) * overtime)
                {
                    package.RetryCount += 1;
                    Console.WriteLine($"超时重发,次数:{package.RetryCount}");
                    SendSafeAsync(package.BufferData, package.TargetEndpoint);  // 重新发送数据
                }
            }
            CheckOutTime();  // 递归调用形成循环检测
        }

        /// <summary>
        /// 异步接收消息任务
        /// </summary>
        public async void AsyncReceiveTask()
        {
            Console.WriteLine($"{udpClient.Client.LocalEndPoint}");
            while (udpClient != null)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    awaitHandle.Enqueue(result);  // 将接收结果加入处理队列
                    Console.WriteLine("接受到了消息");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// 安全异步发送方法
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendSafeAsync(byte[] data, IPEndPoint tragetEndPoint)
        {
            if (udpClient == null || data == null || data.Length == 0)
            {
                return false;
            }
            try
            {
                int sentBytes = await udpClient.SendAsync(data, data.Length, tragetEndPoint);
                Console.WriteLine($"{tragetEndPoint}");
                if (sentBytes == data.Length); // 验证是否全部发送
                Console.WriteLine("发送成功");
                return true;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine("目标不可达（ICMP错误）");// 目标不可达（ICMP错误）
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

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        async Task Handle()
        {   
            while (ct.Token.IsCancellationRequested == false){
                if (awaitHandle.Count > 0 && awaitHandle.TryDequeue(out UdpReceiveResult result))
                {
                    try
                    {
                        BufferEntity bufferEntity =
                            BufferFactory.creatEntityByResult(result.RemoteEndPoint, result.Buffer);
                        Console.WriteLine("处理消息");

                        // 根据消息类型进行不同处理
                        switch (bufferEntity.MessageType)
                        {
                            case MessageType.ACK:
                                HandleAckPacket(bufferEntity); // 处理ACK确认包
                                break;

                            case MessageType.Login:
                                if (bufferEntity.SessionId != NetworkManager.Instance.client.sessionID) continue;
                                NetworkManager.Instance.client.HandleLogicPackage(bufferEntity); // 处理登录逻辑
                                SendAckPacket(bufferEntity); // 发送ACK确认
                                break;

                            case MessageType.CONNECT:
                                HandleConnect(bufferEntity); // 处理连接请求
                                SendAckPacket(bufferEntity);
                                break;

                            default:
                                LogUnknownMessageType(bufferEntity.MessageType); // 未知消息类型
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
            
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void HandleConnect(BufferEntity bufferEntity)
        {
            NetworkManager.Instance.client.setConnect(bufferEntity);
        }

        /// <summary>
        /// 发送ACK确认包
        /// </summary>
        private void SendAckPacket(BufferEntity bufferEntity)
        {
            BufferEntity ackBuffer = BufferFactory.creatAckBuffer(bufferEntity);
            SendSafeAsync(ackBuffer.BufferData, ackBuffer.TargetEndpoint);
        }

        /// <summary>
        /// 处理ACK确认包
        /// </summary>
        private void HandleAckPacket(BufferEntity bufferEntity)
        {
            if (sendPackage.TryRemove(bufferEntity.SequenceNumber, out bufferEntity))
            {
                Console.WriteLine($"收到ACK确认报文,序号是:{bufferEntity.SequenceNumber}");
            }
        }
        
        /// <summary>
        /// 记录未知消息类型
        /// </summary>
        private void LogUnknownMessageType(MessageType bufferEntityMessageType)
        {
            Console.WriteLine("收到未知消息");
        }

        /// <summary>
        /// 关闭UDP连接
        /// </summary>
        public void Close()
        {
            ct.Cancel();
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
        }

        /// <summary>
        /// 发送消息（自动加入重发队列）
        /// </summary>
        public void Send(BufferEntity bufferEntity)
        { 
            sendPackage.TryAdd(bufferEntity.SequenceNumber, bufferEntity);  // 加入发送缓存
            SendSafeAsync(bufferEntity.BufferData, bufferEntity.TargetEndpoint);  // 发送数据
        }
    }
}