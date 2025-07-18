using BlueSeaGameFramework.server.Network.Transport;

namespace BlueSeaGameFramework.server.Network.Client
{
    /// <summary>
    /// 网络管理器（静态类）
    /// 职责：封装客户端网络连接、消息发送和事件处理的核心功能
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    { 
                    // 网络客户端实例
        public NetEvent netEvent;  // 网络事件处理器
        ConcurrentDictionary<int, UClient> clients;
        public UdpTransport uSocket;
        static int myprot;

        public static void SetCog(int myPort)
        {
            myprot = myPort;
        }
        /// <summary>
        /// 初始化网络管理器
        /// </summary>
        public void Init()
        {
            uSocket = new UdpTransport(myprot);
            clients = new ConcurrentDictionary<int, UClient>();
            netEvent = NetEvent.Instance;
        }


        /// <summary>
        /// 发送Protobuf消息
        /// </summary>
        /// <param name="MsgId">消息ID枚举</param>
        /// <param name="iMessage">Protobuf消息对象</param>
        public void Send(MessageId MsgId, IMessage iMessage, int sessionId)
        {
            GetClient(sessionId).Send(MsgId, iMessage);
        }

        /// <summary>
        /// 注册消息事件处理器
        /// </summary>
        /// <typeparam name="T">消息类型（必须实现IMessage）</typeparam>
        /// <param name="msgId">消息ID</param>
        /// <param name="handler">消息处理回调</param>
        public void AddEventHandler<T>(MessageId msgId, Action<MessageWrapper<T>> handler) 
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
        /// 释放网络资源
        /// </summary>
        public void Dispose()
        {
            foreach (var client in clients.Values)
            {
                client.Dispose();
            }
            clients.Clear();

            if (uSocket!=null)
            {
                uSocket.Close();
                uSocket = null;
            }
            netEvent?.Clear();
            netEvent = null;
        }
        
        //移除掉客户端的接口
        public void RemoveClient(int sessionid) {
            UClient client;
            if (clients.TryRemove(sessionid,out client))
            {
                client.Dispose();
                client = null;
            }
        
        }
        public UClient GetClient(int sessionid) {
            UClient client;
            if (clients.TryGetValue(sessionid, out client))
            {
                Console.WriteLine($"{clients.Count}");
                return client;
            }
            
            Console.WriteLine($"{clients.Keys.First()}");
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

        private int sessionId = 1;
        public void setConnect(BufferEntity bufferEntity)
        {
            sessionId++;
            UClient client = new UClient(bufferEntity.OriginEndpoint, sessionId, bufferEntity.SequenceNumber);
            clients.TryAdd(sessionId, client);
        }
    }
}