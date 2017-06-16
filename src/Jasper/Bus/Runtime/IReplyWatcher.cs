using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime
{
    public interface IReplyWatcher
    {
        void Handle(Envelope envelope);
        void Remove(string id);
        Task<T> StartWatch<T>(string id, TimeSpan timeout);
        int Count { get; }
    }
}