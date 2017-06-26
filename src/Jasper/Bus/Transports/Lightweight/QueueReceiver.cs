using System;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Transports.Lightweight
{
    public class QueueReceiver : IDisposable
    {
        private readonly ActionBlock<Message> _block;
        public string QueueName { get; }

        public QueueReceiver(string queueName, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node, IInMemoryQueue inmemory)
        {
            QueueName = queueName;
            var receiver = new Receiver(pipeline, channels, node);

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = node.MaximumParallelization
            };


            _block = new ActionBlock<Message>(m =>
            {
                var callback = new LightweightCallback(this, inmemory);
                return receiver.Receive(m.Data, m.Headers, callback);
            }, options);
        }

        public void Enqueue(Message message)
        {
            _block.Post(message);
        }

        public void Dispose()
        {
            _block.Complete();
        }
    }



}
