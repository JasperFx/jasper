using JasperBus.Model;

namespace JasperBus
{
    public interface IHandlerPolicy
    {
        void Apply(HandlerGraph graph);
    }
}