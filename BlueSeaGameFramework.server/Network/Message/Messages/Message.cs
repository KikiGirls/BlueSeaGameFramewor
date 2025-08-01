using ProtoBuf;


[ProtoContract]
[ProtoInclude(100, typeof(QText))]
[ProtoInclude(101, typeof(PlayerMOve))]
public  class IMessage
{ // 可以放一些通用字段，比如协议版本等
}

[ProtoContract]
public class QText : IMessage
{
    [ProtoMember(1)]
    public int id;
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