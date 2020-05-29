using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IListener : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        IAsyncEnumerable<Envelope> Consume();
        Task<bool> Acknowledge(Envelope envelope);
    }
}
