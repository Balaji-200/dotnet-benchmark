using Google.Protobuf;

namespace LocationService.Benchmark;

public static class Serializer
{
    public static ByteString Serialize(IMessage message)
    {
        return MessageExtensions.ToByteString(message);
    }

    public static T Deserialize<T>(ByteString bytes) where T : IMessage
    {
        T t = (T)Activator.CreateInstance(typeof(T));
        MessageExtensions.MergeFrom(t, bytes);
        return t;
    }
}