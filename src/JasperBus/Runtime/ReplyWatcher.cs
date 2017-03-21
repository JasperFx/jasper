using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Baseline;

namespace JasperBus.Runtime
{
    public interface IReplyWatcher
    {
        void Handle(Envelope envelope);
        void Remove(string id);
        Task<T> StartWatch<T>(string id, TimeSpan timeout);
        int Count { get; }
    }

    public class ReplyWatcher : IReplyWatcher
    {
        private readonly ConcurrentDictionary<string, IReplyListener> _listeners
            = new ConcurrentDictionary<string, IReplyListener>();

        public void Handle(Envelope envelope)
        {
            if (envelope.ResponseId.IsEmpty()) return;

            if (_listeners.ContainsKey(envelope.ResponseId))
            {
                _listeners[envelope.ResponseId].Handle(envelope);
            }
        }

        public void Remove(string id)
        {
            IReplyListener listener;
            _listeners.TryRemove(id, out listener);
        }

        public Task<T> StartWatch<T>(string id, TimeSpan timeout)
        {
            var listener = new ReplyListener<T>(this, id, timeout);
            _listeners[id] = listener;

            return listener.Task;
        }

        public int Count => _listeners.Count;
    }

    public interface IReplyListener
    {
        void Handle(Envelope envelope);
    }

    public class ReplyListener<T> : IReplyListener
    {
        private readonly ReplyWatcher _watcher;
        private readonly TaskCompletionSource<T> _completion;
        private readonly string _originalId;
        private Task _timeout;

        public ReplyListener(ReplyWatcher watcher, string originalId, TimeSpan timeout)
        {
            _watcher = watcher;
            _completion = new TaskCompletionSource<T>();
            ExpiresAt = DateTime.UtcNow.Add(timeout);
            _originalId = originalId;

            _timeout = System.Threading.Tasks.Task.Delay(timeout).ContinueWith(t => {
                if (!_completion.Task.IsCompleted && !_completion.Task.IsFaulted)
                {
                    _completion.SetException(new TimeoutException());
                }
            });

            _completion.Task.ContinueWith(x =>
            {
                _watcher.Remove(_originalId);
                IsExpired = true;
            });
        }

        public Task<T> Task => _completion.Task;



        public void Handle(Envelope envelope)
        {
            if (envelope.ResponseId != _originalId) return;

            if (envelope.Message is T)
            {
                _completion.SetResult((T) envelope.Message);
                _watcher.Remove(_originalId);

                IsExpired = true;
            }

            var ack = envelope.Message as FailureAcknowledgement;
            if (ack == null || ack.CorrelationId != _originalId) return;

            _completion.SetException(new ReplyFailureException(ack.Message));

            _watcher.Remove(_originalId);

        }

        protected bool Equals(ReplyListener<T> other)
        {
            return string.Equals(_originalId, other._originalId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReplyListener<T>) obj);
        }

        public override int GetHashCode()
        {
            return (_originalId != null ? _originalId.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Reply watcher for {0} with Id {1}",typeof(T).FullName, _originalId);
        }

        public bool IsExpired { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
    }

    public class ReplyFailureException : Exception
    {
        public ReplyFailureException(string message) : base(message)
        {
        }
    }
}