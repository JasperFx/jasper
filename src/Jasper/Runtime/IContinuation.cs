using System;
using System.Threading.Tasks;

namespace Jasper.Runtime
{
    // SAMPLE: IContinuation
    public interface IContinuation
    {
        Task Execute(IMessageContext context, DateTime utcNow);
    }

    // ENDSAMPLE
}
