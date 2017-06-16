using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;

namespace Jasper.Testing.Bus.Transports
{
    public class MessageTracker
    {
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        public void Record(object message, Envelope envelope)
        {
            var messageType = message.GetType();
            var list = _waiters[messageType];

            list.Each(x => x.SetResult(envelope));

            list.Clear();
        }

        public Task<Envelope> WaitFor<T>()
        {
            var source = new TaskCompletionSource<Envelope>();
            _waiters[typeof(T)].Add(source);

            return source.Task;
        }
    }
}
