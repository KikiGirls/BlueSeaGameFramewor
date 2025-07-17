namespace BlueSeaGameFramework.Network.Core
{
    /// <summary>
    /// UDP传输层接口
    /// 定义UDP网络通信的核心功能
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// 关闭UDP连接并释放资源
        /// </summary>
        void Close();

        /// <summary>
        /// 处理接收到的消息（从待处理队列中取出并处理）
        /// </summary>
        void Handle();

        /// <summary>
        /// 发送消息（自动加入重传队列）
        /// </summary>
        /// <param name="bufferEntity">要发送的消息实体</param>
        void Send(BufferEntity bufferEntity);

        /// <summary>
        /// 安全异步发送方法（无重传机制）
        /// </summary>
        /// <param name="data">要发送的字节数据</param>
        /// <returns>是否发送成功</returns>
        Task<bool> SendSafeAsync(byte[] data);
    }

    /// <summary>
    /// UDP传输层扩展接口（内部使用）
    /// 包含需要对外隐藏但接口实现类需要的方法
    /// </summary>
    internal interface IUdpTransportInternal : ITransport
    {
        /// <summary>
        /// 处理ACK确认包（内部实现）
        /// </summary>
        /// <param name="bufferEntity">包含ACK确认的消息实体</param>
        void HandleAckPacket(BufferEntity bufferEntity);

        /// <summary>
        /// 处理连接请求（内部实现）
        /// </summary>
        /// <param name="bufferEntity">连接请求消息实体</param>
        void HandleConnect(BufferEntity bufferEntity);
    }
}