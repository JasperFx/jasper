using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public interface IMissingHandler
    {
        void Handle(Envelope envelope);
    }
}
