using Jasper.Bus.Model;

namespace Jasper.Bus
{
    public interface IHandlerPolicy
    {
        void Apply(HandlerGraph graph);
    }
}