using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Baseline;
using Jasper.Util;

namespace Jasper.Bus.Runtime
{
    public class ReplyWatcher : IReplyWatcher
    {
        private readonly ConcurrentDictionary<Guid, IReplyListener> _listeners
            = new ConcurrentDictionary<Guid, IReplyListener>();

        public void Handle(Envelope envelope)
        {
            if (envelope.ResponseId.IsEmpty()) return;

            if (_listeners.ContainsKey(envelope.ResponseId))
            {
                _listeners[envelope.ResponseId].Handle(envelope);
            }
        }

        public void Remove(Guid id)
        {
            IReplyListener listener;
            _listeners.TryRemove(id, out listener);
        }

        public Task<T> StartWatch<T>(Guid id, TimeSpan timeout)
        {
            var listener = new ReplyListener<T>(this, id, timeout);
            _listeners[id] = listener;

            return listener.Task;
        }

        public int Count => _listeners.Count;
    }
}
