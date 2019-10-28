using Jasper.Configuration;
using Lamar;

namespace Jasper.Persistence
{
    public interface ITransactionFrameProvider
    {
        void ApplyTransactionSupport(IChain chain, IContainer container);
    }
}
