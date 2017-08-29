using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.LightningDb.Transport
{
    public class PersistentReceiver : IDisposable
    {
        private readonly LightningDbPersistence _persistence;
        private readonly IHandlerPipeline _pipeline;
        private readonly ChannelGraph _channels;
        private readonly ChannelNode _node;
        private readonly ActionBlock<Envelope> _block;
        private Task _persisted;
        public string QueueName { get; }

        public PersistentReceiver(string queueName, LightningDbPersistence persistence, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node)
        {
            _persistence = persistence;
            _pipeline = pipeline;
            _channels = channels;
            _node = node;
            QueueName = queueName;

            var receiver = new Receiver(pipeline, channels, node);

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = node.MaximumParallelization
            };


            _block = new ActionBlock<Envelope>(m => receive(receiver, m), options);
        }

        public void LoadPersisted(CancellationToken token)
        {
            _persisted = Task.Factory.StartNew(() =>
            {
                _persistence.ReadAll(QueueName, e => _block.Post(e));
            }, token);
        }

        private Task receive(Receiver receiver, Envelope m)
        {
            var callback = new PersistentCallback(m, this);
            return receiver.Receive(m.Data, m.Headers, callback);
        }

        public void Enqueue(Envelope message)
        {
            _block.Post(message);
        }

        public void Dispose()
        {
            _block.Complete();
        }

        public class PersistentCallback : IMessageCallback
        {
            private readonly Envelope _envelope;
            private readonly PersistentReceiver _parent;

            public PersistentCallback(Envelope envelope, PersistentReceiver parent)
            {
                _envelope = envelope;
                _parent = parent;
            }

            public Task MarkSuccessful()
            {
                _parent._persistence.Remove(_parent.QueueName, _envelope);
                return Task.CompletedTask;
            }

            public Task MarkFailed(Exception ex)
            {
                _parent._persistence.Remove(_parent.QueueName, _envelope);
                return Task.CompletedTask;
            }

            public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
            {
                _parent._persistence.Remove(_parent.QueueName, _envelope);
                delayedJobs.Enqueue(time, envelope);

                return Task.CompletedTask;
            }

            public Task MoveToErrors(ErrorReport report)
            {
                // There's an outstanding issue for actually doing error reports
                return Task.CompletedTask;
            }

            public Task Requeue(Envelope envelope)
            {
                _parent._persistence.Replace(_parent.QueueName, envelope);
                _parent._block.Post(envelope);

                return Task.CompletedTask;
            }

            // TODO -- let's make this smart enough to be able to transfer
            public Task Send(Envelope envelope)
            {
                throw new NotSupportedException();
            }

            public bool SupportsSend { get; } = false;
            public string TransportScheme { get; } = "lq.tcp";
        }


    }
}
