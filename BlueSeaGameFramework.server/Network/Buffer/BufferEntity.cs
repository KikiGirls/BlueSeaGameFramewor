namespace BlueSeaGameFramework.server.Network.Buffer
{
    /// <summary>
    /// 网络数据包实体类，用于封装网络通信协议数据
    /// </summary>
    public class BufferEntity 
    {
        // 协议头固定大小(字节)
        private const int ProtocolHeaderSize = 32;
        
        // 网络相关属性
        /// <summary>
        /// 远程终结点(IP地址和端口)
        /// </summary>
        /// public IPEndPoint receiverEndpoint { get; set; }
        public IPEndPoint TargetEndpoint;

        public IPEndPoint OriginEndpoint;
    
        #region 协议头和数据区
        /// <summary>
        /// 协议数据部分的大小(字节)
        /// </summary>
        public int ProtocolSize;
        
        /// <summary>
        /// 会话ID，标识通信会话
        /// </summary>
        public int SessionId { get; set; }
        
        /// <summary>
        /// 序列号，用于消息排序和匹配
        /// </summary>
        public int SequenceNumber { get; set; }
        
        /// <summary>
        /// 模块ID，标识业务模块
        /// </summary>
        public int ModuleId { get; set; }
        
        /// <summary>
        /// 数据包发送时间(UTC时间)
        /// </summary>
        public DateTime SendTime { get; set; }
        
        /// <summary>
        /// 消息类型(枚举)
        /// </summary>
        public MessageType MessageType { get; set; }
        
        /// <summary>
        /// 消息ID
        /// </summary>
        public MessageId MessageId { get; set; }
    
        /// <summary>
        /// 业务协议数据
        /// </summary>
        public byte[] ProtocolData { get; set; }
        #endregion 
    
        /// <summary>
        /// 完整的网络传输数据(包含协议头和业务数据)
        /// </summary>
        public byte[] BufferData { get; set; }
    
        // 内部状态
        public int RetryCount = 0;
       

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public BufferEntity(IPEndPoint targetEndPoint, int sendSn)
        {
            TargetEndpoint = targetEndPoint;
            ProtocolSize = 0;
            SessionId = 0;
            SequenceNumber =sendSn;
            ModuleId = 0;
            MessageType = MessageType.CONNECT;
            MessageId = MessageId.None;
            ProtocolData = null;
            SendTime = DateTime.UtcNow; // 使用UTC时间
            
        }

        /// <summary>
        /// 创建新的数据包实体
        /// </summary>
        /// <param name="endPoint">远程终结点</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="sequenceNumber">序列号</param>
        /// <param name="moduleId">模块ID</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="messageId">消息ID</param>
        /// <param name="protocolData">业务数据</param>
        public BufferEntity(int sessionId, int sequenceNumber, 
            int moduleId, MessageType messageType, MessageId messageId, 
            byte[] protocolData)
        {
            ProtocolSize = protocolData.Length;
            SessionId = sessionId;
            SequenceNumber = sequenceNumber;
            ModuleId = moduleId;
            MessageType = messageType;
            MessageId = messageId;
            ProtocolData = protocolData ?? throw new ArgumentNullException(nameof(protocolData));
            SendTime = DateTime.UtcNow; // 使用UTC时间
        }

        /// <summary>
        /// 将数据序列化为网络数据包
        /// </summary>
        /// <param name="isAck">是否是ACK确认包</param>
        /// <returns>序列化后的字节数组</returns>
        public byte[] SerializeToNetworkPacket(bool isAck)
        {
            // 计算有效载荷大小(ACK包没有业务数据)
            int payloadSize = isAck ? 0 : ProtocolSize;
            int totalSize = ProtocolHeaderSize + payloadSize;
            byte[] data = new byte[totalSize];
    
            using (var ms = new MemoryStream(data))
            using (var writer = new BinaryWriter(ms))
            {
                // 写入协议头(共32字节)
                writer.Write(payloadSize);        // 4字节 - 有效载荷大小
                writer.Write(SessionId);         // 4字节 - 会话ID
                writer.Write(SequenceNumber);    // 4字节 - 序列号
                writer.Write(ModuleId);          // 4字节 - 模块ID
                writer.Write(SendTime.Ticks);    // 8字节 - 发送时间(UTC ticks)
                writer.Write((int)MessageType);  // 4字节 - 消息类型(枚举转int)
                writer.Write((int)MessageId);         // 4字节 - 消息ID
        
                // 如果不是ACK包且存在业务数据，写入业务数据
                if (!isAck && ProtocolData != null)
                {
                    writer.Write(ProtocolData);
                }
            }
    
            BufferData = data;
            return data;
        }
        
        /// <summary>
        /// 从网络数据包解析出实体对象
        /// </summary>
        /// <param name="data">网络数据包</param>
        /// <returns>解析后的BufferEntity对象</returns>
        /// <exception cref="ArgumentException">数据包无效时抛出</exception>
        /// <exception cref="InvalidDataException">数据包损坏时抛出</exception>
        public static BufferEntity ParseFromNetworkPacket(byte[] data)
        {
            // 基本校验
            if (data == null || data.Length < ProtocolHeaderSize)
                throw new ArgumentException("无效的网络数据包");
    
            var entity = new BufferEntity();
    
            try
            {
                using (var ms = new MemoryStream(data))
                using (var reader = new BinaryReader(ms))
                {
                    // 读取协议大小
                    entity.ProtocolSize = reader.ReadInt32();
                    
                    // 验证数据长度是否足够
                    int expectedLength = ProtocolHeaderSize + entity.ProtocolSize;
                    if (data.Length < expectedLength)
                        throw new InvalidDataException("数据包不完整");
            
                    // 读取协议头各字段
                    entity.SessionId = reader.ReadInt32();
                    entity.SequenceNumber = reader.ReadInt32();
                    entity.ModuleId = reader.ReadInt32();
                    entity.SendTime = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
                    entity.MessageType = (MessageType)reader.ReadInt32();
                    entity.MessageId = (MessageId)reader.ReadInt32();
                    
                    // 读取业务数据(如果有)
                    if (entity.ProtocolSize > 0)
                    {
                        entity.ProtocolData = reader.ReadBytes(entity.ProtocolSize);
                    }
                }
        
                return entity;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("解析网络数据包失败", ex);
            }
        }
        
        /// <summary>
        /// 创建ACK确认包的构造函数
        /// </summary>
        /// <param name="package">需要确认的原始数据包</param>
        public BufferEntity(BufferEntity package)
        {
            TargetEndpoint = package.OriginEndpoint;
            // 复制协议头字段
            ProtocolSize = 0; // ACK包没有业务数据
            
            SessionId = package.SessionId;
            SequenceNumber = package.SequenceNumber;
            ModuleId = package.ModuleId;
            SendTime = DateTime.UtcNow; // 使用当前UTC时间
            MessageType = MessageType.ACK;    // 设置为ACK类型
            MessageId = package.MessageId;
    
            // 直接序列化为网络数据包
            BufferData = SerializeToNetworkPacket(isAck:true);
        }

        public BufferEntity()
        {
        }
    }    
}