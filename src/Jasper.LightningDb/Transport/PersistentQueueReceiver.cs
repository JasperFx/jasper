using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.LightningDb.Transport
{
    public class PersistentQueueReceiver : IDisposable
    {
        private readonly ActionBlock<Message> _block;

        public PersistentQueueReceiver(string queueName, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node)
        {
            QueueName = queueName;
            var receiver = new Receiver(pipeline, channels, node);

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = node.MaximumParallelization
            };


            _block = new ActionBlock<Message>(m => receive(receiver, m), options);
        }

        public string QueueName { get; }

        public void Enqueue(Message message)
        {
            // This is assuming that the database work happens somewhere else
            _block.Post(message);
        }

        private Task receive(Receiver receiver, Message m)
        {
            var callback = new PersistentCallback(m, _block);
            return receiver.Receive(m.Data, m.Headers, (IMessageCallback) callback);
        }

        public void Dispose()
        {
            _block.Complete();
        }
    }
}