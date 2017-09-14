using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus
{
    // SAMPLE: IMissingHandler
    public interface IMissingHandler
    {
        Task Handle(Envelope envelope, IEnvelopeContext context);
    }
    // ENDSAMPLE
}
