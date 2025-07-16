using System.Collections.Concurrent;

namespace BlueSeaGameFramework.server.Network.Client
{
    // NetEvent 类作为单例管理网络事件的注册与分发
    public class NetEvent : Singleton<NetEvent>
    {
        // 存储每个消息 ID 对应的事件处理程序，采用线程安全的 ConcurrentDictionary
        private readonly ConcurrentDictionary<MessageId, Delegate> eventHandlers = new();

        /// <summary>
        /// 注册事件处理程序，确保每个消息 ID 只对应一个处理程序
        /// </summary>
        /// <typeparam name="T">消息类型，必须实现 IMessage 接口</typeparam>
        /// <param name="msgId">事件的唯一标识符</param>
        /// <param name="handler">处理事件的委托</param>
        public void AddEventHandler<T>(MessageId msgId, Action<MessageWrapper<T>> handler) where T :  new()
        {
            // 参数检查
            if (msgId == null) throw new ArgumentNullException(nameof(msgId));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            // 使用 AddOrUpdate 确保每个消息 ID 只对应一个处理程序
            eventHandlers.AddOrUpdate(msgId, handler, (_, __) => handler);
        }

        /// <summary>
        /// 移除某个消息 ID 对应的事件处理程序
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="msgId">要移除事件处理程序的消息 ID</param>
        public void RemoveEventHandler<T>(MessageId msgId,  Action<MessageWrapper<T>> handler)
        {
            // 参数检查
            if (msgId == null) throw new ArgumentNullException(nameof(msgId));

            // 从字典中移除指定的消息 ID 及其对应的处理程序
            eventHandlers.TryRemove(msgId, out _);
        }

        /// <summary>
        /// 根据消息 ID 分发消息并触发相应的处理程序
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="msgId">事件的唯一标识符</param>
        /// <param name="message">要分发的消息</param>
        public void Dispatch<T>(MessageId msgId, MessageWrapper<T> message) where T : new()
        {
            // 参数检查
            if (msgId == null) throw new ArgumentNullException(nameof(msgId));

            // 查找消息 ID 对应的处理程序并执行
            if (eventHandlers.TryGetValue(msgId, out var handler))
            {
                // 执行消息处理程序
                ((Action<MessageWrapper<T>>)handler)?.Invoke(message);
            }
        }

        /// <summary>
        /// 清空所有的事件处理程序
        /// </summary>
        public void Clear()
        {
            // 清空所有注册的事件处理程序
            eventHandlers.Clear();
        }
    }
}
