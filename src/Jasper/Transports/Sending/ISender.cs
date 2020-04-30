using System;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public interface ISender : IDisposable
    {
        Uri Destination { get; }
        
        Task Send(Envelope envelope);
    }
}
