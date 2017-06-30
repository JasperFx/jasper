using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Lightweight
{
    public class SendingAgent : IDisposable
    {
        private ISenderCallback _callback;
        private BatchingBlock<Envelope> _outgoing;
        private ActionBlock<OutgoingMessageBatch> _sender;
        private ActionBlock<Envelope[]> _grouper;

        public void Start(ISenderCallback callback)
        {
            _callback = callback;
            _grouper = new ActionBlock<Envelope[]>(_ => groupMessages(_));
            _outgoing = new BatchingBlock<Envelope>(200, _grouper);

            _sender = new ActionBlock<OutgoingMessageBatch>(sendBatch, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });

        }


        private void groupMessages(Envelope[] messages)
        {
            var groups = messages.GroupBy(x => x.Destination);
            foreach (var @group in groups)
            {
                var batch = new OutgoingMessageBatch(@group.Key, @group);
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

                    using (var stream = client.GetStream())
                    {
                        await WireProtocol.Send(stream, batch, _callback).TimeoutAfter(5000);
                    }
                }
                catch (Exception e)
                {
                    batchSendFailed(batch, e);
                }
            }


        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception exception)
        {
            _callback.ProcessingFailure(batch, exception);
        }

        private Task connect(TcpClient client, Uri destination)
        {
            return Dns.GetHostName() == destination.Host || destination.Host == "localhost" || destination.Host == "127.0.0.1"
                ? client.ConnectAsync(IPAddress.Loopback, destination.Port)
                : client.ConnectAsync(destination.Host, destination.Port);
        }

        public bool Enqueue(Envelope message)
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
