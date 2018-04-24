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
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        private readonly ConcurrentBag<ITracker> _trackers = new ConcurrentBag<ITracker>();

        public void Record(object message, Envelope envelope)
        {
            foreach (var tracker in _trackers)
            {
                tracker.Check(envelope, message);
            }

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

    public class CountTracker<T> : ITracker
    {
        private readonly int _expected;
        private readonly List<ITracker> _trackers;
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
        private int _count = 0;

        public CountTracker(int expected, List<ITracker> trackers)
        {
            _expected = expected;
            _trackers = trackers;
        }

        public Task<bool> Completion => _completion.Task;
        public void Check(Envelope envelope, object message)
        {
            if (message is T)
            {
                Interlocked.Increment(ref _count);

                if (_count >= _expected)
                {
                    _completion.TrySetResult(true);
                    _trackers.Remove(this);
                }
            }
        }
    }
}
