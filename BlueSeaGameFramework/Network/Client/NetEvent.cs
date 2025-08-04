        public void Dispatch<T>(MessageId msgId, BufferEntity message)
        {
            // 参数检查
            if (msgId == null) throw new ArgumentNullException(nameof(msgId));
            if (message == null) throw new ArgumentNullException(nameof(message));

            // 查找消息 ID 对应的处理程序并执行
            if (eventHandlers.TryGetValue(msgId, out var handler))
            {
                // 执行消息处理程序
                if (handler is Action<BufferEntity> action)
                {
                    action(message);
                }
                else
                {
                    throw new InvalidOperationException($"事件处理程序类型不匹配，实际类型为: {handler.GetType()}");
                }
            }
            else
            {
                throw new KeyNotFoundException($"未找到消息ID为{msgId}的事件处理程序");
            }
        }
