using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Tracking
{
    public class MessageHistory
    {
        private readonly object _lock = new object();
        private readonly IList<TaskCompletionSource<MessageTrack[]>> _waiters = new List<TaskCompletionSource<MessageTrack[]>>();
        private readonly Dictionary<Type, TaskCompletionSource<MessageTrack>> _backgroundWaiters = new Dictionary<Type, TaskCompletionSource<MessageTrack>>();

        private readonly IList<MessageTrack> _completed = new List<MessageTrack>();
        private readonly Dictionary<string, MessageTrack> _outstanding = new Dictionary<string, MessageTrack>();

        public Task<MessageTrack[]> Watch(Action action)
        {
            var waiter = new TaskCompletionSource<MessageTrack[]>();

            lock (_lock)
            {
                _waiters.Clear();

                _completed.Clear();
                _outstanding.Clear();

                _waiters.Add(waiter);
            }

            action();

            return waiter.Task;
        }


        public Task<MessageTrack> WaitFor<T>()
        {
            var waiter = new TaskCompletionSource<MessageTrack>();
            lock (_lock)
            {
                _backgroundWaiters.Add(typeof(T), waiter);
            }
            return waiter.Task;
        }

        public async Task<TaskCompletionSource<MessageTrack[]>> WatchAsync(Func<Task> func)
        {
            var waiter = new TaskCompletionSource<MessageTrack[]>();

            lock (_lock)
            {
                _waiters.Clear();

                _completed.Clear();
                _outstanding.Clear();

                _waiters.Add(waiter);
            }

            await func();

            return waiter;
        }

        public void Complete(Envelope envelope, string activity, Exception ex = null)
        {
            var key = MessageTrack.ToKey(envelope, activity);
            var messageType = envelope.Message?.GetType();
            lock (_lock)
            {
                if (_outstanding.ContainsKey(key))
                {
                    var track = _outstanding[key];
                    _outstanding.Remove(key);

                    track.Finish(envelope, ex);

                    _completed.Add(track);

                    processCompletion();
                }
                else if (messageType != null && _backgroundWaiters.ContainsKey(messageType))
                {
                    var waiter = _backgroundWaiters[messageType];
                    _backgroundWaiters.Remove(messageType);
                    var track = new MessageTrack(envelope, activity);
                    track.Finish(envelope, ex);
                    waiter.SetResult(track);

                }
            }
        }

        public void Start(Envelope envelope, string activity)
        {
            var track = new MessageTrack(envelope, activity);
            lock (_lock)
            {
                if (_outstanding.ContainsKey(track.Key))
                {
                    _outstanding[track.Key] = track;
                }
                else
                {
                    _outstanding.Add(track.Key, track);
                }
            }
        }

        private void processCompletion()
        {
            if (_outstanding.Count == 0 && _completed.Count > 0)
            {
                var tracks = _completed.ToArray();

                foreach (var waiter in _waiters)
                {
                    waiter.SetResult(tracks);
                }

                _waiters.Clear();
            }
        }

    }
}
