using BlueSeaGameFramework.Network;
using Google.Protobuf;
using System;

namespace BlueSeaGameFramework.Network.Client
{
    /// <summary>
    /// 网络管理器接口（面向抽象编程）
    /// </summary>
    public interface INetworkManager : IDisposable
    {
        void Connect(string ip, int port);
        void Send(IMessageId MsgId, IMessage message);
        void AddEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage, new();
        void RemoveEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage;
        void Tick();
    }

    /// <summary>
    /// 网络事件处理器接口
    /// </summary>
    public interface INetEvent
    {
        void AddEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage, new();
        void RemoveEventHandler<T>(IMessageId msgId, Action<MessageWrapper<T>> handler) where T : IMessage;
        void Clear();
    }

    /// <summary>
    /// 网络客户端接口
    /// </summary>
    public interface IUClient : IDisposable
    {
        void Connect(string ip, int port);
        void Send(IMessageId MsgId, IMessage message);
        void Tick();
    }


}