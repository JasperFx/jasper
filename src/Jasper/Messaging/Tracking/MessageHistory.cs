using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Util;

namespace Jasper.Messaging.Tracking
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

                _exceptions.Clear();

                _waiters.Add(waiter);
            }

            action();

            return waiter.Task;
        }


        public Task<MessageTrack> WaitFor<T>(int timeoutInMilliseconds = 5000)
        {
            var waiter = new TaskCompletionSource<MessageTrack>();
            lock (_lock)
            {
                _backgroundWaiters.Add(typeof(T), waiter);
            }
            return waiter.Task.TimeoutAfter(5000);
        }

        public async Task<MessageTrack[]> WatchAsync(Func<Task> func, int timeoutInMilliseconds = 5000)
        {
            var waiter = new TaskCompletionSource<MessageTrack[]>();


            lock (_lock)
            {
                _waiters.Clear();

                _completed.Clear();
                _outstanding.Clear();

                _exceptions.Clear();

                _waiters.Add(waiter);
            }

            await func();

            return await waiter.Task.TimeoutAfter(timeoutInMilliseconds);
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
                var tracks = _completed.Distinct().ToArray();

                foreach (var waiter in _waiters)
                {
                    waiter.SetResult(tracks);
                }

                _waiters.Clear();
            }
        }

        private readonly IList<Exception> _exceptions = new List<Exception>();
        public void LogException(Exception exception)
        {
            _exceptions.Add(exception);
        }

        public void AssertNoExceptions()
        {
            if (_exceptions.Any())
            {
                throw new AggregateException(_exceptions);
            }
        }


    }
}
