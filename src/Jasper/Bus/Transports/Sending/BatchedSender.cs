using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.Util;

namespace Jasper.Bus.Transports.Sending
{
    public class BatchedSender : ISender
    {
        public Uri Destination { get; }

        private readonly ISenderProtocol _protocol;
        private readonly CancellationToken _cancellation;
        private ISenderCallback _callback;
        private ActionBlock<Envelope[]> _sender;
        private BatchingBlock<Envelope> _outgoing;
        private int _queued = 0;

        public BatchedSender(Uri destination, ISenderProtocol protocol, CancellationToken cancellation)
        {
            Destination = destination;
            _protocol = protocol;
            _cancellation = cancellation;
        }

        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            _sender = new ActionBlock<Envelope[]>(sendBatch, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _cancellation
            });

            // TODO -- We *could* stick another block between the batching and sending
            // to do the serialization ahead of time
            _outgoing = new BatchingBlock<Envelope>(200, _sender, _cancellation);
        }

        public int QueuedCount => _queued + _outgoing.ItemCount;

        private async Task sendBatch(Envelope[] envelopes)
        {
            _queued += envelopes.Length;

            var batch = new OutgoingMessageBatch(Destination, envelopes);

            // TODO -- this needs to be temporary. There's a huge potential performance gain
            // if we separate the serialization, even the serialization to the raw message bytes
            // to a separate action block
            try
            {
                foreach (var envelope in batch.Messages)
                {
                    envelope.EnsureData();
                }
            }
            catch (Exception e)
            {
                batchSendFailed(batch, e);
            }

            try
            {



                await _protocol.SendBatch(_callback, batch);
            }
            catch (Exception e)
            {
                batchSendFailed(batch, e);
            }
            finally
            {
                _queued -= envelopes.Length;
            }
        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception exception)
        {
            _callback.ProcessingFailure(batch, exception);
        }

        public Task Enqueue(Envelope message)
        {
            if (_outgoing == null) throw new InvalidOperationException("This agent has not been started");

            _outgoing.Post(message);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _sender?.Complete();
            _outgoing?.Dispose();
        }
    }
}
