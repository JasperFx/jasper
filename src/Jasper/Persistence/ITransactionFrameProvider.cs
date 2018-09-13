using Jasper.Configuration;

namespace Jasper.Persistence
{
    public interface ITransactionFrameProvider
    {
        void ApplyTransactionSupport(IChain chain);
    }
}