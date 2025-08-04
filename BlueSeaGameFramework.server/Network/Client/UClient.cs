

using BlueSeaGameFramework.server.Network;

namespace BlueSeaGameFramework.server
{
    /// <summary>
    /// UDP客户端核心类，管理与服务器的UDP通信
    /// </summary>
    public class UClient
    {
        // 客户端终端地址
        public IPEndPoint ClinetEndPoint;
        
        // UDP传输层实例
        public UdpTransport uSocket;
        
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
        public UClient(IPEndPoint clinetEndPoint, int SessionID, int handleSN)
        {
            sessionID = SessionID;
            isConnected = true;
            uSocket = NetworkManager.Instance.USocket;
            ClinetEndPoint = clinetEndPoint;
            sendSN = 0;  // 初始化发送序列号
            this.handleSN = handleSN;
            waitHandle = new ConcurrentDictionary<int, BufferEntity>();
            Debug.Log($"创建新客户端连接，客户端端口号{clinetEndPoint}，分配的会话id{sessionID},初始handleSN:{handleSN}，初始sendSN:{sendSN}");
            Connect();
        }



        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        { // 关闭UDP连接
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
            BufferEntity bufferEntity = BufferFactory.CreateEntityForSend(msgId, iMessage, ClinetEndPoint, sendSN, sessionID);  // 创建发送实体
            uSocket.Send(bufferEntity);  // 通过UDP传输层发送
        }
        public void SendBroadcastBuffer(BufferEntity bufferEntity)
        {
            sendSN += 1;
            BufferEntity broadcastBuffer = BufferFactory.CreateEntityForBroadcast(ClinetEndPoint, sendSN, sessionID, bufferEntity.ProtocolData, bufferEntity.MessageId,bufferEntity.ProtocolSize);
            uSocket.Send(broadcastBuffer);
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
            Debug.Log($"收到来自客户端的逻辑报文，报文类型{bufferEntity.MessageId},会话id{bufferEntity.SessionId},sn{bufferEntity.SequenceNumber}");
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
            //直接转发了
            Debug.Log("收到的是逻辑报文，将逻辑报文转发给其余玩家");
            NetworkManager.Instance.Broadcast(bufferEntity);
        }

        /// <summary>
        /// 设置连接状态（由服务器响应连接请求时调用）
        /// </summary>
        /// <param name="bufferEntity">包含连接信息的实体</param>
        public void setConnect(BufferEntity bufferEntity)
        {
            isConnected = true;  // 标记为已连接
        }

        public void Connect()
        {
            BufferEntity bufferEntity = BufferFactory.CreateConnectEntity(ClinetEndPoint,sendSN,sessionID);
            Debug.Log("回复客户端同意连接报文");
            uSocket.Send(bufferEntity);
        }


    }
}