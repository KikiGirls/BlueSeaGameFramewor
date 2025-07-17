using BlueSeaGameFramework.server.Network.Transport;

namespace BlueSeaGameFramework.server.Network.Client
{
    /// <summary>
    /// UDP客户端核心类，管理与服务器的UDP通信
    /// </summary>
    public class UClient
    {
        // 服务器终端地址
        public IPEndPoint ServerEndPoint;
        
        // UDP传输层实例
        UdpTransport uSocket;
        
        // 会话ID（由服务器分配）
        public int sessionID;
        
        // 发送序列号（自增）
        public int sendSN;
        
        // 已处理的最大序列号（用于乱序处理）
        public int handleSN;
        
        // 连接状态标志
        bool isConnected = false;
        
        // 待处理消息缓存（用于处理乱序到达的消息）
        private ConcurrentDictionary<int, BufferEntity> waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serverEndPoint">服务器终结点</param>
        /// <param name="Myprot">本地绑定端口</param>
        public UClient(IPEndPoint serverEndPoint, int Myprot)
        {
            uSocket = new UdpTransport(Myprot);
            ServerEndPoint = serverEndPoint;
            sendSN = 0;  // 初始化发送序列号
            waitHandle = new ConcurrentDictionary<int, BufferEntity>();
        }

        /// <summary>
        /// 连接服务器（发送连接请求）
        /// </summary>
        public void ConnectServer()
        {
            BufferEntity bufferEntity = BufferFactory.creatConnectEntity(ServerEndPoint, sendSN);
            uSocket.Send(bufferEntity);  // 发送连接请求
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            uSocket.Close();  // 关闭UDP连接
            isConnected = false;  // 重置连接状态
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <param name="iMessage">消息体</param>
        public void Send(MessageId msgId, IMessage iMessage)
        {
            sendSN += 1;  // 递增序列号
            BufferEntity bufferEntity = BufferFactory.creatEntityForSend(msgId, iMessage, ServerEndPoint, sendSN, sessionID);  // 创建发送实体
            uSocket.Send(bufferEntity);  // 通过UDP传输层发送
        }

        /// <summary>
        /// 主循环更新方法（待实现）
        /// </summary>
        public void Tick()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 处理逻辑数据包（处理乱序和重复）
        /// </summary>
        /// <param name="bufferEntity">接收到的数据实体</param>
        public void HandleLogicPackage(BufferEntity bufferEntity)
        {
            // 检查是否为已处理过的旧包
            if (bufferEntity.SequenceNumber <= handleSN)
            {
                return;
            }

            // 检查是否是不连续的包（需要缓存）
            if (bufferEntity.SequenceNumber > handleSN + 1)
            {
                if (waitHandle.TryAdd(bufferEntity.SequenceNumber, bufferEntity))
                {
                    Console.WriteLine("收到错序的报文");
                }
                return;
            }
            
            // 处理当前包
            handleSN = bufferEntity.SequenceNumber;
            Dispatch(bufferEntity);  // 分发处理

            // 检查是否有后续包可以处理
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN + 1, out nextBuffer))
            {
                // 递归处理后续包
                HandleLogicPackage(nextBuffer);
            }
        }

        /// <summary>
        /// 分发消息到业务逻辑层
        /// </summary>
        /// <param name="bufferEntity">要处理的数据实体</param>
        private void Dispatch(BufferEntity bufferEntity)
        {
            MessageId messageId = bufferEntity.MessageId;
            var messageWrapper = new MessageWrapper<BufferEntity>(bufferEntity);
            NetworkManager.Instance.netEvent.Dispatch(messageId, messageWrapper);  // 通过事件系统分发
        }

        /// <summary>
        /// 设置连接状态（由服务器响应连接请求时调用）
        /// </summary>
        /// <param name="bufferEntity">包含连接信息的实体</param>
        public void setConnect(BufferEntity bufferEntity)
        {
            isConnected = true;  // 标记为已连接
            sessionID = bufferEntity.SessionId;  // 记录会话ID
            handleSN = bufferEntity.SequenceNumber; // 初始化已处理序列号
            Console.WriteLine($"已连接服务器，会话id{sessionID},初始化序列号{handleSN}");
        }
    }
}