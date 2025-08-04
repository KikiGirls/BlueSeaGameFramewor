namespace BlueSeaGameFramework.server.Network
{
    /// <summary>
    /// UDP传输层实现类，负责UDP通信的核心功能
    /// </summary>
    public class UdpTransport
    {
        UdpClient _udpClient;                      // UDP客户端实例
        ConcurrentQueue<UdpReceiveResult> _awaitHandle;  // 待处理消息队列
        ConcurrentDictionary<(int, int), BufferEntity> _sendPackage; // 缓存已发送的报文（用于超时重传）
        CancellationTokenSource _ct = new CancellationTokenSource();
        TimeSpan _overtime = TimeSpan.FromMilliseconds(150);  // 超时时间设定为150ms

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="myPort">本地绑定端口</param>
        public UdpTransport(int myPort)
        {
            _udpClient = new UdpClient(myPort);
            _awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
            _sendPackage = new ConcurrentDictionary<(int ,int), BufferEntity>();
            AsyncReceiveTask();  // 启动异步接收任务
            Task.Run(CheckOutTime, _ct.Token);// 启动超时检测任务
            Task.Run(Handle, _ct.Token);
        }

        /// <summary>
        /// 超时检测方法（循环执行）
        /// </summary>
        private async Task CheckOutTime()
        {
            while (!_ct.Token.IsCancellationRequested)
            {
                await Task.Delay(_overtime, _ct.Token);
                var keysToRemove = new List<(int SessionId, int SequenceNumber)>();
                foreach (var package in _sendPackage.Values)
                {
                    if (package.RetryCount >= 10)
                    {
                        Console.WriteLine($"重发次数超过10次,关闭对应Clinet连接:{package.SessionId}");
                        NetworkManager.Instance.RemoveClient(package.SessionId);
                        keysToRemove.Add((package.SessionId, package.SequenceNumber));
                    }
                    else if (DateTime.Now - package.SendTime >= (package.RetryCount + 1) * _overtime)
                    {
                        package.RetryCount += 1;
                        Console.WriteLine($"超时重发,次数:{package.RetryCount}");
                        await SendSafeAsync(package.BufferData, package.TargetEndpoint);  // 重新发送数据
                    }
                }
                foreach (var key in keysToRemove)
                {
                    _sendPackage.TryRemove(key, out _); // 使用 `out _` 忽略返回值
                }
            }
        }

        /// <summary>
        /// 异步接收消息任务
        /// </summary>
        public async void AsyncReceiveTask()
        {
            Debug.Log("AsyncReceiveTask()正在监听");
            Debug.Log($"正在监听的EndPoint{_udpClient.Client.LocalEndPoint}");
            while (!_ct.Token.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    _awaitHandle.Enqueue(result); // 将接收结果加入处理队列、
                    Debug.Log("接受到了来自客户端的消息");
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
        /// <param name="targetEndPoint">目标终结点</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendSafeAsync(byte[] data, IPEndPoint targetEndPoint)
        {
            if (data == null || data.Length == 0)
                return false;
            try
            {
                int sentBytes = await _udpClient.SendAsync(data, data.Length, targetEndPoint);
                Debug.Log($"尝试向端口：{targetEndPoint}，发送数据包");
                if (sentBytes == data.Length)
                {
                    Debug.Log($"成功，向{targetEndPoint}发送数据包");
                }
                else
                {
                    Debug.LogError($"失败，向{targetEndPoint}发送数据包");
                }
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

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        private async Task Handle()
        {
            while (!_ct.Token.IsCancellationRequested)
            {
                if (_awaitHandle.Count > 0 && _awaitHandle.TryDequeue(out UdpReceiveResult result))
                {
                    try
                    {
                        BufferEntity bufferEntity = BufferFactory.CreateEntityByResult(result.RemoteEndPoint, result.Buffer);
                        Console.WriteLine("处理收到的数据包，将其转为BufferEntity，交给相应模块处理");
                        switch (bufferEntity.MessageType)
                        {
                            case MessageType.ACK:
                                HandleAckPacket(bufferEntity);  // 处理ACK确认包
                                break;
                            case MessageType.Login:
                                await SendAckPacket(bufferEntity);
                                NetworkManager.Instance.GetClient(bufferEntity.SessionId).HandleLogicPackage(bufferEntity); // 处理登录逻辑
                                break;
                            case MessageType.CONNECT:
                                HandleConnect(bufferEntity);  // 处理连接请求
                                await SendAckPacket(bufferEntity);
                                break;
                            default:
                                LogUnknownMessageType();  // 未知消息类型
                                break;
                        }
                    }
                    catch (InvalidDataException ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                }
                else
                {
                    await Task.Delay(10); // 防止空转
                }
            }
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void HandleConnect(BufferEntity bufferEntity)
        {
            NetworkManager.Instance.SetConnect(bufferEntity);
        }

        /// <summary>
        /// 发送ACK确认包
        /// </summary>
        private async Task SendAckPacket(BufferEntity bufferEntity)
        {
            Debug.Log($"收到消息发送对应ack，来自客户端sn：{bufferEntity.SessionId}，消息序号是:{bufferEntity.SequenceNumber}");
            BufferEntity ackBuffer = BufferFactory.CreateAckBuffer(bufferEntity);
            await SendSafeAsync(ackBuffer.BufferData, ackBuffer.TargetEndpoint);
        }

        /// <summary>
        /// 处理ACK确认包
        /// </summary>
        private void HandleAckPacket(BufferEntity bufferEntity)
        {
            BufferEntity removed;
            if (_sendPackage.TryRemove((bufferEntity.SessionId, bufferEntity.SequenceNumber), out removed))
            {
                Debug.Log($"已处理，收到ACK确认报文,来自客户端sn：{bufferEntity.SessionId}，消息序号是:{bufferEntity.SequenceNumber}d");
            }
        }
        /// <summary>
        /// 记录未知消息类型
        /// </summary>
        private void LogUnknownMessageType()
        {
            Debug.LogError("收到未知消息");
        }

        /// <summary>
        /// 关闭UDP连接
        /// </summary>
        public void Close()
        {
            _ct.Cancel();
            _udpClient.Close();
        }

        /// <summary>
        /// 发送消息（自动加入重发队列）
        /// </summary>
        public async Task Send(BufferEntity bufferEntity)
        { 
            _sendPackage.TryAdd((bufferEntity.SessionId, bufferEntity.SequenceNumber), bufferEntity);  // 加入发送缓存
            await SendSafeAsync(bufferEntity.BufferData, bufferEntity.TargetEndpoint);  // 发送数据
        }
    }
}