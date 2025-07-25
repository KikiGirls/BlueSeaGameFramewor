namespace BlueSeaGameFramework.server.Utils;


          
using Google.Protobuf;
using System;
using System.ComponentModel;
using System.IO;

public static class ProtobufHelper
{
    public static byte[] ToBytes(object message)
    {
        return ((Google.Protobuf.IMessage)message).ToByteArray();
    }

    public static void ToStream(object message, MemoryStream stream)
    {
        ((Google.Protobuf.IMessage)message).WriteTo(stream);
    }

    public static object FromBytes(Type type, byte[] bytes, int index, int count)
    {
        object message = Activator.CreateInstance(type);

        ((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
        ISupportInitialize iSupportInitialize = message as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return message;
        }
        iSupportInitialize.EndInit();
        return message;
    }

    public static object FromBytes(object instance, byte[] bytes, int index, int count)
    {
        object message = instance;
        ((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
        ISupportInitialize iSupportInitialize = message as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return message;
        }
        iSupportInitialize.EndInit();
        return message;
    }

    public static T FromBytes<T>(byte[] bytes) where T : Google.Protobuf.IMessage
    {
        object message = Activator.CreateInstance(typeof(T));
        ((Google.Protobuf.IMessage)message).MergeFrom(bytes, 0, bytes.Length);
        ISupportInitialize iSupportInitialize = message as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return (T)message;
        }
        iSupportInitialize.EndInit();
        return (T)message;
    }



    public static object FromStream(Type type, MemoryStream stream)
    {
        object message = Activator.CreateInstance(type);
        ((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
        ISupportInitialize iSupportInitialize = message as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return message;
        }
        iSupportInitialize.EndInit();
        return message;
    }

    public static object FromStream(object message, MemoryStream stream)
    {
        // 这个message可以从池中获取，减少gc
        ((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
        ISupportInitialize iSupportInitialize = message as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return message;
        }
        iSupportInitialize.EndInit();
        return message;
    }
}

    
