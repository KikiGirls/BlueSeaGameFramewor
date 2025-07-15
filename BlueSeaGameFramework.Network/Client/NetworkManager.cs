

namespace BlueSeaGameFramework.Network.Client
{
    /// <summary>
    /// 网络管理器（静态类）
    /// 职责：封装客户端网络连接、消息发送和事件处理的核心功能
    /// </summary>
    public static class NetworkManager
    {
        private static UClient client;      // 网络客户端实例
        private static NetEvent netEvent;    // 网络事件处理器

        /// <summary>
        /// 初始化网络管理器
        /// </summary>
        public static void Init()
        {
            client = new UClient();
            netEvent = NetEvent.Instance;
        }

        /// <summary>
        /// 连接到指定服务器
        /// </summary>
        /// <param name="ip">服务器IP地址</param>
        /// <param name="port">服务器端口号</param>
        public static void Connect(string ip, int port) => client.Connect(ip, port);

        /// <summary>
        /// 发送Protobuf消息
        /// </summary>
        /// <param name="MsgId">消息ID枚举</param>
        /// <param name="iMessage">Protobuf消息对象</param>
        public static void Send(IMessageId MsgId, IMessage iMessage) => client.Send(MsgId, iMessage);

        /// <summary>
        /// 注册消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型（必须实现IMessage）</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public static void AddEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) 
            where T : IMessage, new()
            => netEvent.AddEventHandler(msgId, handler);

        /// <summary>
        /// 移除消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public static void RemoveEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) 
            where T : IMessage
            => netEvent.RemoveEventHandler(msgId, handler);

        /// <summary>
        /// 网络轮询（需在主线程定期调用）
        /// </summary>
        public static void Tick() => client.Tick();

        /// <summary>
        /// 释放网络资源
        /// </summary>
        public static void Dispose()
        {
            client?.Dispose();
            client = null;
            netEvent?.Clear();
            netEvent = null;
        }

        /// <summary>
        /// 重置网络管理器（先释放再初始化）
        /// </summary>
        public static void Reset()
        {
            Dispose();
            Init();
        }
    }
}