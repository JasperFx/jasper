using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus
{

    public interface IQueueExpression
    {
        IQueueExpression MaximumParallelization(int number);

    }

    public interface ITransportExpression
    {
        IQueueExpression Queue(string queueName);
        void Disable();

        ITransportExpression ListenOnPort(int port);

        IQueueExpression DefaultQueue { get; }

        ITransportExpression MaximumSendAttempts(int number);
    }

    public interface IQueueIndexer
    {
        QueueSettings this[string queueName] { get; }

        bool Has(string queueName);
    }

    public class TransportSettings : IQueueIndexer,IEnumerable<QueueSettings>
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

    public interface IQueueSettings
    {
        // TODO -- will grow someday to include routing of messages to incoming queues

        IQueueSettings MaximumParallelization(int maximumParallelHandlers    );
        IQueueSettings SingleThreaded();
    }

    public class QueueSettings : IQueueSettings
    {
        // Mostly for informative reasons
        public Uri Uri { get; set; }
        public string Name { get; }

        public QueueSettings(string name)
        {
            Name = name;
        }

        public int Parallelization { get; set; } = 5;

        IQueueSettings IQueueSettings.MaximumParallelization(int maximumParallelHandlers    )
        {
            Parallelization = maximumParallelHandlers;
            return this;
        }

        IQueueSettings IQueueSettings.SingleThreaded()
        {
            Parallelization = 1;
            return this;
        }
    }

    public enum TransportState
    {
        Enabled,
        Disabled
    }
}
