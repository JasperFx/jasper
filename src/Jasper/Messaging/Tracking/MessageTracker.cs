using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Tracking
{
    public class MessageTracker
    {
        private readonly ConcurrentBag<ITracker> _trackers = new ConcurrentBag<ITracker>();

        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t =>
                new List<TaskCompletionSource<Envelope>>());

        public void Record(object message, Envelope envelope)
        {
            foreach (var tracker in _trackers) tracker.Check(envelope, message);

            var messageType = message.GetType();
            var list = _waiters[messageType];

            list.Each(x => x.SetResult(envelope));

            list.Clear();
        }

        public Task<Envelope> WaitFor<T>()
        {
            var source = new TaskCompletionSource<Envelope>();
            _waiters[typeof(T)].Add(source);

            Task.Delay(30.Seconds()).ContinueWith(x => { source.TrySetCanceled(); });

            return source.Task;
        }
    }

    public interface ITracker
    {
        void Check(Envelope envelope, object message);
    }

}
