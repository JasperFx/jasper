using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Queues.Net;

namespace Jasper.Bus.Queues.New
{
    public interface ISendingAgent
    {
        void RetryIn(OutgoingMessage message, TimeSpan delay);
    }


    public class SendingAgent : IDisposable
    {
        private ISenderCallback _callback;
        private BatchingBlock<OutgoingMessage> _outgoing;
        private ActionBlock<OutgoingMessageBatch> _sender;
        private ActionBlock<OutgoingMessage[]> _grouper;

        public void Start(ISenderCallback callback)
        {
            _callback = callback;
            _grouper = new ActionBlock<OutgoingMessage[]>(_ => groupMessages(_));
            _outgoing = new BatchingBlock<OutgoingMessage>(200, _grouper);

            _sender = new ActionBlock<OutgoingMessageBatch>(sendBatch, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });

        }


        private void groupMessages(OutgoingMessage[] messages)
        {
            var groups = messages.GroupBy(x => x.Destination);
            foreach (var @group in groups)
            {
                var batch = new OutgoingMessageBatch(@group.Key, group, new TcpClient());
                _sender.Post(batch);
            }
        }


        private async Task sendBatch(OutgoingMessageBatch batch)
        {
            using (var client = new TcpClient())
            {
                try
                {
                    await connect(client, batch.Destination)
                        .TimeoutAfter(5000);

                    await WireProtocol.Send(client.GetStream(), batch, _callback)
                        .TimeoutAfter(5000);
                }
                catch (Exception e)
                {
                    batchSendFailed(batch, e);
                }
            }


        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception exception)
        {
            throw new NotImplementedException();
        }

        private Task connect(TcpClient client, Uri destination)
        {
            return Dns.GetHostName() == destination.Host
                ? client.ConnectAsync(IPAddress.Loopback, destination.Port)
                : client.ConnectAsync(destination.Host, destination.Port);
        }

        public bool Enqueue(OutgoingMessage message)
        {
            if (_outgoing == null) throw new InvalidOperationException("This agent has not been started");

            _outgoing.Post(message);

            return true;
        }

        public void Dispose()
        {
            _sender.Complete();
            _grouper.Complete();
            _outgoing?.Dispose();
        }
    }






}
