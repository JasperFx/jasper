using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Settings
{
    public class TransportSettings : ITransportExpression, IQueueIndexer, IEnumerable<QueueSettings>
    {
        private readonly LightweightCache<string, QueueSettings> _queues
            = new LightweightCache<string, QueueSettings>(name => new QueueSettings(name));

        public IQueueIndexer Queues => this;

        QueueSettings IQueueIndexer.this[string queueName] => _queues[queueName];

        bool IQueueIndexer.Has(string queueName)
        {
            return _queues.Has(queueName);
        }

        public string Protocol { get; }

        public TransportSettings(string protocol)
        {
            Protocol = protocol;
            _queues.FillDefault(TransportConstants.Default);
            _queues.FillDefault(TransportConstants.Replies);
        }

        public int? Port { get; set; }
        public int MaximumSendAttempts { get; set; }
        public TransportState State { get; set; } = TransportState.Enabled;

        public IQueueSettings Queue(string queueName)
        {
            return _queues[queueName];
        }

        public void Disable()
        {
            State = TransportState.Disabled;
        }

        public ITransportExpression ListenOnPort(int port)
        {
            Port = port;
            return this;
        }

        IQueueSettings ILoopbackTransportExpression.DefaultQueue => _queues[TransportConstants.Default];

        ITransportExpression ITransportExpression.MaximumSendAttempts(int number)
        {
            MaximumSendAttempts = number;
            return this;
        }

        public QueueSettings DefaultQueue => _queues[TransportConstants.Default];

        internal string[] AllQueueNames()
        {
            return _queues.GetAll().Select(x => x.Name).ToArray();
        }

        public IEnumerator<QueueSettings> GetEnumerator()
        {
            return _queues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
