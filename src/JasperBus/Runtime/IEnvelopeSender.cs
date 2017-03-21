using System.Collections.Generic;

namespace JasperBus.Runtime
{
    // Tested strictly through integration tests
    public interface IEnvelopeSender
    {
        string Send(Envelope envelope);
        string Send(Envelope envelope, IMessageCallback callback);
    }
}