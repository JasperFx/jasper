using System;
using System.Threading.Tasks;

namespace JasperBus.Runtime.Invocation
{
    public interface IContinuation
    {
        Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow);
    }
}