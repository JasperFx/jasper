using System;
using System.Threading.Tasks;

namespace Jasper.Remotes.Messaging
{
    public interface IEventAggregator
    {
        void SendMessage<T>(T message);
        ResponseExpression Send<T>(T message);
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly IMessagingHub _messaging;

        public EventAggregator(IMessagingHub messaging)
        {
            _messaging = messaging;
        }

        public void SendMessage<T>(T message)
        {
            try
            {
                _messaging.Send(message);
            }
            catch (Exception e)
            {
                // THIS IS IMPORTANT, NO FAILURES CAN POSSIBLY GET OUT HERE
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor = color;
            }
        }

        public ResponseExpression Send<T>(T message)
        {
            return new ResponseExpression(_messaging, () => SendMessage(message));
        }
    }

    public class ResponseExpression
    {
        private readonly IMessagingHub _messaging;
        private readonly Action _sendAction;

        public ResponseExpression(IMessagingHub messaging, Action sendAction)
        {
            _messaging = messaging;
            _sendAction = sendAction;
        }

        public Task<T> AndWaitFor<T>()
        {
            var watcher = new ResponseWatcher<T>(_messaging);
            _messaging.AddListener(watcher);

            _sendAction();

            return watcher.Task;
        }
    }

    public class ResponseWatcher<T> : IListener<T>
    {
        private readonly IMessagingHub _messaging;
        private readonly TaskCompletionSource<T> _task;

        public ResponseWatcher(IMessagingHub messaging)
        {
            _messaging = messaging;
            _task = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _task.Task;

        public void Receive(T message)
        {
            _task.SetResult(message);
            _messaging.RemoveListener(this);
        }
    }
}
