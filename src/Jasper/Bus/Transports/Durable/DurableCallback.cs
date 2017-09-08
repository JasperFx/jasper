using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.Durable
{
    public class DurableCallback : IMessageCallback
    {
        private readonly string _queueName;
        private readonly Envelope _envelope;
        private readonly IPersistence _persistence;
        private readonly Action<Envelope> _requeue;

        public DurableCallback(string queueName, Envelope envelope, IPersistence persistence, Action<Envelope> requeue)
        {
            _queueName = queueName;
            _envelope = envelope;
            _persistence = persistence;
            _requeue = requeue;
        }

        public Task MarkSuccessful()
        {
            _persistence.Remove(_queueName, _envelope);
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            _persistence.Remove(_queueName, _envelope);
            return Task.CompletedTask;
        }

        public Task MoveToErrors(ErrorReport report)
        {
            // There's an outstanding issue for actually doing error reports
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            _persistence.Replace(_queueName, envelope);
            _requeue(envelope);

            return Task.CompletedTask;
        }

        // TODO -- let's make this smart enough to be able to transfer
        public Task Send(Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public bool SupportsSend { get; } = false;
        public string TransportScheme { get; } = "durable";
    }
}
