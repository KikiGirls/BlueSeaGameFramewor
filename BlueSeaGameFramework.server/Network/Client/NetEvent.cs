namespace BlueSeaGameFramework.server
{
    /// <summary>
    /// NetEvent 类作为单例管理网络事件的注册与分发
    /// </summary>
    public class NetEvent : Singleton<NetEvent>
    {
        // 存储每个消息 ID 对应的事件处理程序，采用线程安全的 ConcurrentDictionary
        private readonly ConcurrentDictionary<MessageId, Action<BufferEntity>> _eventHandlers = new();

        /// <summary>
        /// 注册事件处理程序，确保每个消息 ID 只对应一个处理程序
        /// </summary>
        /// <param name="msgId">事件的唯一标识符</param>
        /// <param name="handler">处理事件的委托</param>
        public void AddEventHandler(MessageId msgId, Action<BufferEntity> handler)
        {
            // 参数检查
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            // 使用 AddOrUpdate 确保每个消息 ID 只对应一个处理程序
            _eventHandlers.AddOrUpdate(msgId, handler, (_, _) => handler);
        }

        /// <summary>
        /// 移除某个消息 ID 对应的事件处理程序
        /// </summary>
        /// <param name="msgId">要移除事件处理程序的消息 ID</param>
        /// <param name="handler">要移除的处理程序（可选参数，用于验证）</param>
        public void RemoveEventHandler(MessageId msgId)
        {
            // 从字典中移除指定的消息 ID 及其对应的处理程序
            _eventHandlers.TryRemove(msgId, out _);
        }

        /// <summary>
        /// 根据消息 ID 分发消息并触发相应的处理程序
        /// </summary>
        /// <param name="msgId">事件的唯一标识符</param>
        /// <param name="message">要分发的消息</param>
        public void Dispatch(MessageId msgId, BufferEntity message)
        {
            // 参数检查
            if (message == null) throw new ArgumentNullException(nameof(message));

            // 查找消息 ID 对应的处理程序并执行
            if (_eventHandlers.TryGetValue(msgId, out var handler))
            {
                try
                {
                    // 执行消息处理程序
                    handler(message);
                }
                catch (Exception ex)
                {
                    // 记录异常但不中断程序执行
                    Console.WriteLine($"处理消息 {msgId} 时发生异常: {ex.Message}");
                    throw; // 根据需要可以选择是否重新抛出异常
                }
            }
            else
            {
                Console.WriteLine($"警告: 未找到消息ID {msgId} 对应的处理程序");
            }
        }

        /// <summary>
        /// 检查是否已注册指定消息ID的处理程序
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <returns>如果已注册返回true，否则返回false</returns>
        public bool HasHandler(MessageId msgId)
        {
            return _eventHandlers.ContainsKey(msgId);
        }

        /// <summary>
        /// 获取当前注册的处理程序数量
        /// </summary>
        public int HandlerCount => _eventHandlers.Count;

        /// <summary>
        /// 清空所有的事件处理程序
        /// </summary>
        public void Clear()
        {
            // 清空所有注册的事件处理程序
            _eventHandlers.Clear();
        }
    }
}
