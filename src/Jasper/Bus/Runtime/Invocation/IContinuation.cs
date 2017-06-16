using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Invocation
{
    // SAMPLE: IContinuation
    public interface IContinuation
    {
        Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow);
    }
    // ENDSAMPLE
}