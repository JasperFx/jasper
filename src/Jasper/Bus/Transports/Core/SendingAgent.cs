using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Util;

namespace Jasper.Bus.Transports.Core
{
    public class SendingAgent : IDisposable
    {
        private readonly ISenderProtocol _protocol;
        private ISenderCallback _callback;
        private BatchingBlock<Envelope> _outgoing;
        private ActionBlock<OutgoingMessageBatch> _sender;
        private ActionBlock<Envelope[]> _grouper;

        public SendingAgent() : this(new SocketSenderProtocol())
        {
        }

        public SendingAgent(ISenderProtocol protocol)
        {
            _protocol = protocol;
        }

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
            try
            {
                await _protocol.SendBatch(_callback, batch);
            }
            catch (Exception e)
            {
                batchSendFailed(batch, e);
            }
        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception exception)
        {
            _callback.ProcessingFailure(batch, exception);
        }

        public bool Enqueue(Envelope message)
        {
            if (_outgoing == null) throw new InvalidOperationException("This agent has not been started");

            _outgoing.Post(message);

            return true;
        }

        public void Dispose()
        {
            _sender?.Complete();
            _grouper?.Complete();
            _outgoing?.Dispose();
        }
    }






}
