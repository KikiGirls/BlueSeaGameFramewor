using ProtoBuf;

#region Imessage
[ProtoContract]
[ProtoInclude(5, typeof(QText))]
[ProtoInclude(6, typeof(PlayerMOve))]
[ProtoInclude(101, typeof(GameInitEventMsg))]
[ProtoInclude(102, typeof(GameStartEventMsg))]
[ProtoInclude(103, typeof(PlayerTurnChangeEventMsg))]
public class IMessage
{ // 可以放一些通用字段，比如协议版本等
}


#endregion





#region 客户端发送给服务器的消息
[ProtoContract]
public class EndcurrentTurnEventMsg : IMessage
{
    [ProtoMember(1)]
    public PlayerName NewPlayerName; // 当前玩家名称
    [ProtoMember(2)]
    public int TurnNumber; // 当前回合数
}

[ProtoContract]
public class QText : IMessage
{
    [ProtoMember(1)]
    public int id;
}

[ProtoContract]
public class PlayerMoveEventMsg : IMessage
{
    [ProtoMember(1)]
    public PlayerName PlayerName; // 玩家名称
    [ProtoMember(2)]
    public float x; // x坐标
    [ProtoMember(3)]
    public float y; // y坐标
    [ProtoMember(4)]
    public float z; // z坐标
}

[ProtoContract]
public class PlayerMOve : IMessage
{
    [ProtoMember(1)]
    public int Playerid;

    [ProtoMember(2)] public float x;
    
    [ProtoMember(3)] public float y;
    
    [ProtoMember(4)] public float z;
}


[ProtoContract]
public class GameInitEventMsg : IMessage
{
    [ProtoMember(1)]
    public PlayerName PlayerName;//需要发送给服务器的本地玩家名称
}


#endregion


#region 服务端发送给客户端的消息


[ProtoContract]
public class GameStartEventMsg : IMessage
{
}


[ProtoContract]
public class PlayerTurnChangeEventMsg : IMessage
{
    [ProtoMember(1)]
    public PlayerName NewPlayerName; // 当前玩家名称
    [ProtoMember(2)]
    public int TurnNumber; // 当前回合数
}



#endregion
