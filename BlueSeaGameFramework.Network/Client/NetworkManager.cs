

using System.Net;

namespace BlueSeaGameFramework.Network.Client
{
    /// <summary>
    /// 网络管理器（静态类）
    /// 职责：封装客户端网络连接、消息发送和事件处理的核心功能
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        public UClient client;      // 网络客户端实例
        public NetEvent netEvent;  // 网络事件处理器

        static IPEndPoint serverEndPoint;
        static int myprot;

        static public void SetCog(int myPort, string serverIp, int serverPort)
        {
            myprot = myPort;
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }
        /// <summary>
        /// 初始化网络管理器
        /// </summary>
        public  void Init()
        {
            client = new UClient(serverEndPoint, myprot);
            netEvent = NetEvent.Instance;
        }

        /// <summary>
        /// 连接到指定服务器
        /// </summary>
        /// <param name="ip">服务器IP地址</param>
        /// <param name="port">服务器端口号</param>
        public  void Connect(string ip, int port) => client.ConnectServer();

        /// <summary>
        /// 发送Protobuf消息
        /// </summary>
        /// <param name="MsgId">消息ID枚举</param>
        /// <param name="iMessage">Protobuf消息对象</param>
        public  void Send(MessageId MsgId, IMessage iMessage) => client.Send(MsgId, iMessage);

        /// <summary>
        /// 注册消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型（必须实现IMessage）</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public  void AddEventHandler<T>(MessageId msgId, Action<MessageWrapper<T>> handler) 
            where T : IMessage, new()
            => netEvent.AddEventHandler(msgId, handler);

        /// <summary>
        /// 移除消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public  void RemoveEventHandler<T>(MessageId msgId, Action<MessageWrapper<T>> handler) 
            where T : IMessage
            => netEvent.RemoveEventHandler(msgId, handler);

        /// <summary>
        /// 网络轮询（需在主线程定期调用）
        /// </summary>
        public  void Tick() => client.Tick();

        /// <summary>
        /// 释放网络资源
        /// </summary>
        public  void Dispose()
        {
            client?.Dispose();
            client = null;
            netEvent?.Clear();
            netEvent = null;
        }

        /// <summary>
        /// 重置网络管理器（先释放再初始化）
        /// </summary>
        public  void Reset()
        {
            Dispose();
            Init();
        }
    }
}