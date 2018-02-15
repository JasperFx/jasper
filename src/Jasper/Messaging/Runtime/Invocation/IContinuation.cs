using System;
using System.Threading.Tasks;

namespace Jasper.Messaging.Runtime.Invocation
{
    // SAMPLE: IContinuation
    public interface IContinuation
    {
        Task Execute(IMessageContext context, DateTime utcNow);
    }
    // ENDSAMPLE
}