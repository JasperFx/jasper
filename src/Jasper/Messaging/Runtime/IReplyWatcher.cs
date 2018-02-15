using System;
using System.Threading.Tasks;

namespace Jasper.Messaging.Runtime
{
    public interface IReplyWatcher
    {
        void Handle(Envelope envelope);
        void Remove(Guid id);
        Task<T> StartWatch<T>(Guid id, TimeSpan timeout);
        int Count { get; }
    }
}
