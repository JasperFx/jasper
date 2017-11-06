using System;

namespace Jasper.Bus.Transports.Receiving
{
    public interface IListener : IReceiverCallback, IDisposable
    {
        void Start();
    }
}