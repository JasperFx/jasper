using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Remotes.Messaging
{
    public class MessagingHub : IMessagingHub
    {
        // TODO -- need to do some locking on this bad boy
        private readonly IList<object> _listeners = new List<object>();

        public IEnumerable<object> Listeners => _listeners;

        public MessagingHub(IEnumerable<IListener> listeners)
        {
            _listeners.AddRange(listeners);
        }

        public void AddListener(object listener)
        {
            _listeners.Fill(listener);
        }

        public void RemoveListener(object listener)
        {
            _listeners.Remove(listener);
        }

        public void Send<T>(T message)
        {
            _listeners.OfType<IListener<T>>().ToArray().Each(x => x.Receive(message));
        }

        public void SendJson(string json)
        {
            object o = JsonSerialization.DeserializeMessage(json);

            typeof (Sender<>).CloseAndBuildAs<ISender>(o.GetType())
                .Send(o, this);
        }

        public void ClearAll()
        {
            _listeners.Clear();
        }

        public interface ISender
        {
            void Send(object o, MessagingHub listeners);
        }

        public class Sender<T> : ISender
        {
            public void Send(object o, MessagingHub listeners)
            {
                listeners.Send(o.As<T>());
            }
        }
    }
}
