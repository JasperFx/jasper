using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    public interface IHandlerPolicy
    {
        void Apply(HandlerGraph graph);
    }
}