using System;

namespace Jasper.Serialization;

public interface IMessageSerializer
{
    string ContentType { get; }

    // TODO -- use read only memory later, and let it go back to the pool later.
    // "rent memory"
    byte[] Write(object message);

    object ReadFromData(Type messageType, byte[] data);
    object ReadFromData(byte[] data);
}
