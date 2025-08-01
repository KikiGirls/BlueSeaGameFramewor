using ProtoBuf;

public static class ProtobufHelper
{
    public static byte[] toByte<T>(T obj) where T : IMessage
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        return ms.ToArray();
    }

    public static T GetIMessageFormByte<T>(byte[] data) where T : IMessage
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }

}