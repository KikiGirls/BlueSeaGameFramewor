namespace BlueSeaGameFramework.server.Network
{
    /// <summary>
    /// 网络管理器
    /// 职责：封装客户端网络连接、消息发送和事件处理的核心功能
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    { 
        // 网络事件处理器
        public NetEvent? NetEvent;
        private ConcurrentDictionary<int, UClient>? _clients;
        public UdpTransport? USocket;
        private static int _myPort;
        private int _sessionId = 1;

        /// <summary>
        /// 广播消息给除发送者外的所有客户端
        /// </summary>
        /// <param name="bufferEntity">要广播的消息实体</param>
        public void Broadcast(BufferEntity bufferEntity)
        {
            if (bufferEntity == null) throw new ArgumentNullException(nameof(bufferEntity));
            if (_clients == null) return;

            foreach (var clientSessionId in _clients.Keys)
            {
                if (clientSessionId != bufferEntity.SessionId)
                {
                    var client = GetClient(clientSessionId);
                    if (client != null)
                    {
                        client.SendBroadcastBuffer(bufferEntity);
                    }
                }
            }
        }

        /// <summary>
        /// 广播消息给所有在线玩家
        /// </summary>
        /// <param name="msgId">消息ID枚举</param>
        /// <param name="iMessage">Protobuf消息对象</param>
        public void BroadcastToAll(MessageId msgId, IMessage iMessage)
        {
            if (iMessage == null) throw new ArgumentNullException(nameof(iMessage));
            if (_clients == null || _clients.Count == 0) return;

            foreach (var client in _clients.Values)
            {
                try
                {
                    client.Send(msgId, iMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"广播消息给客户端 {client.sessionID} 失败: {ex.Message}");
                }
            }
            
            Console.WriteLine($"已广播消息 {msgId} 给 {_clients.Count} 个客户端");
        }

        /// <summary>
        /// 设置服务器端口
        /// </summary>
        /// <param name="myPort">本地端口号</param>
        public static void SetConfig(int myPort)
        {
            _myPort = myPort;
        }

        /// <summary>
        /// 初始化网络管理器
        /// </summary>
        public void Init()
        {
            if (_myPort == 0)
                throw new InvalidOperationException("请先调用SetConfig设置端口号");

            USocket = new UdpTransport(_myPort);
            _clients = new ConcurrentDictionary<int, UClient>();
            NetEvent = new NetEvent(); // 直接创建新实例而不是使用单例
        }

        /// <summary>
        /// 发送Protobuf消息
        /// </summary>
        /// <param name="msgId">消息ID枚举</param>
        /// <param name="iMessage">Protobuf消息对象</param>
        /// <param name="sessionId">会话ID</param>
        public void Send(MessageId msgId, IMessage iMessage, int sessionId)
        {
            var client = GetClient(sessionId);
            if (client == null)
                throw new ArgumentException($"未找到会话ID为{sessionId}的客户端");
            
            client.Send(msgId, iMessage);
        }

        /// <summary>
        /// 注册消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型（必须实现IMessage）</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public void AddEventHandler(MessageId msgId, Action<BufferEntity> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            NetEvent?.AddEventHandler(msgId, handler);
        }

        /// <summary>
        /// 移除消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public void RemoveEventHandler(MessageId msgId) 
        {
            NetEvent?.RemoveEventHandler(msgId);
        }

        /// <summary>
        /// 释放网络资源
        /// </summary>
        public void Dispose()
        {
            if (_clients != null)
            {
                foreach (var client in _clients.Values)
                {
                    client.Dispose();
                }
                _clients.Clear();
            }

            USocket?.Close();
            NetEvent?.Clear();
        }
        
        /// <summary>
        /// 移除指定客户端连接
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        public void RemoveClient(int sessionId) 
        {
            if (_clients?.TryRemove(sessionId, out var client) == true)
            {
                client.Dispose();
                Console.WriteLine($"已移除客户端连接，会话ID: {sessionId}");
            }
        }

        /// <summary>
        /// 根据会话ID获取客户端
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>客户端实例，如果不存在返回null</returns>
        public UClient? GetClient(int sessionId) 
        {
            if (_clients?.TryGetValue(sessionId, out var client) == true)
            {
                Console.WriteLine($"获取到客户端，会话ID: {sessionId}");
                return client;
            }
            Console.WriteLine($"未获取到客户端，会话ID: {sessionId}");
            return null;
        }

        /// <summary>
        /// 重置网络管理器（先释放再初始化）
        /// </summary>
        public void Reset()
        {
            Dispose();
            Init();
        }

        /// <summary>
        /// 设置客户端连接
        /// </summary>
        /// <param name="bufferEntity">连接请求的缓冲实体</param>
        public void SetConnect(BufferEntity bufferEntity)
        {
            if (bufferEntity == null || bufferEntity.OriginEndpoint == null)
            {
                Console.WriteLine("无效的连接请求：缺少客户端端点信息");
                return;
            }

            var newSessionId = Interlocked.Increment(ref _sessionId);
            var client = new UClient(bufferEntity.OriginEndpoint, newSessionId, 0); // 第三个参数是初始handleSN
            
            if (_clients?.TryAdd(newSessionId, client) == true)
            {
                Console.WriteLine($"新客户端连接成功，会话ID: {newSessionId}，端点: {bufferEntity.OriginEndpoint}");
            }
            else
            {
                Console.WriteLine($"客户端连接失败，会话ID: {newSessionId}");
                client.Dispose();
            }
        }

        /// <summary>
        /// 获取当前连接的客户端数量
        /// </summary>
        public int ClientCount => _clients?.Count ?? 0;

        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public bool IsInitialized => USocket != null && _clients != null && NetEvent != null;
    }
}