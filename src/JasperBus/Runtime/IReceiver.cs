using System.Collections.Generic;

namespace JasperBus.Runtime
{
    public interface IReceiver
    {
        void Receive(byte[] data, Dictionary<string, string> headers, IMessageCallback callback);
    }
}