using System;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues;

public interface ILocalQueue : IReceiver
{
    void Enqueue(Envelope envelope);

    [Obsolete("Just start things up inside a constructor function")]
    void StartListening(IListener listener);
}
