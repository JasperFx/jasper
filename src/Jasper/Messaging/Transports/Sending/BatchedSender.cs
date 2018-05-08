using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.Transports.Util;

namespace Jasper.Messaging.Transports.Sending
{
    public class BatchedSender : ISender
    {
        public Uri Destination { get; }

        private readonly ISenderProtocol _protocol;
        private readonly CancellationToken _cancellation;
        private readonly ITransportLogger _logger;
        private ISenderCallback _callback;
        private ActionBlock<OutgoingMessageBatch> _sender;
        private BatchingBlock<Envelope> _batching;
        private int _queued = 0;
        private ActionBlock<Envelope> _serializing;
        private TransformBlock<Envelope[], OutgoingMessageBatch> _batchWriting;

        public BatchedSender(Uri destination, ISenderProtocol protocol, CancellationToken cancellation, ITransportLogger logger)
        {
            Destination = destination;
            _protocol = protocol;
            _cancellation = cancellation;
            _logger = logger;
        }

        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            _sender = new ActionBlock<OutgoingMessageBatch>(SendBatch, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _cancellation,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });

            _sender.Completion.ContinueWith(x =>
            {
                Console.WriteLine("BatchedSender.Sender Completed");

                if (x.IsFaulted)
                {
                    // TODO -- need to restart things!!!
                    _logger.LogException(x.Exception);
                }
            }, _cancellation);

            _serializing = new ActionBlock<Envelope>(async e =>
            {
                try
                {
                    e.EnsureData();
                    await _batching.SendAsync(e);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message:$"Error while trying to serialize envelope {e}");
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });


            _serializing.Completion.ContinueWith(x =>
            {
                Console.WriteLine("BatchedSender.Serializing Completed");

                if (x.IsFaulted)
                {
                    // TODO -- need to restart things!!!
                    _logger.LogException(x.Exception);
                }
            }, _cancellation);


            _batchWriting = new TransformBlock<Envelope[], OutgoingMessageBatch>(
                envelopes =>
                {
                    var batch = new OutgoingMessageBatch(Destination, envelopes);
                    _queued += batch.Messages.Count;
                    return batch;
                }, new ExecutionDataflowBlockOptions{BoundedCapacity = DataflowBlockOptions.Unbounded, MaxDegreeOfParallelism = 10});

            _batchWriting.Completion.ContinueWith(x =>
            {
                Console.WriteLine("BatchedSender.BatchWriting Completed");

                if (x.IsFaulted)
                {
                    // TODO -- need to restart things!!!
                    _logger.LogException(x.Exception);
                }
            }, _cancellation);

            _batchWriting.LinkTo(_sender);

            _batching = new BatchingBlock<Envelope>(200, _batchWriting, _cancellation);
            _batching.Completion.ContinueWith(x =>
            {
                Console.WriteLine("BatchedSender.Batching Completed");

                if (x.IsFaulted)
                {
                    // TODO -- need to restart things!!!
                    _logger.LogException(x.Exception);
                }
            }, _cancellation);

        }

        public int QueuedCount => _queued + _batching.ItemCount + _serializing.InputCount;

        public bool Latched { get; private set; }
        public Task LatchAndDrain()
        {
            Console.WriteLine("BatchedSender LatchAndDrain");

            Latched = true;

            _sender.Complete();
            _serializing.Complete();
            _batchWriting.Complete();
            _batching.Complete();

            _logger.CircuitBroken(Destination);

            return Task.CompletedTask;
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public Task Ping()
        {
            var batch = OutgoingMessageBatch.ForPing(Destination);
            return _protocol.SendBatch(_callback,batch);
        }

        public async Task SendBatch(OutgoingMessageBatch batch)
        {
            if (_cancellation.IsCancellationRequested) return;

            try
            {
                if (Latched)
                {
                    await _callback.SenderIsLatched(batch);
                }
                else
                {
                    await _protocol.SendBatch(_callback, batch);

                    _logger.OutgoingBatchSucceeded(batch);
                }

            }
            catch (Exception e)
            {
                batchSendFailed(batch, e);
            }

            finally
            {
                _queued -= batch.Messages.Count;
            }
        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception exception)
        {
            _callback.ProcessingFailure(batch, exception);
        }

        public Task Enqueue(Envelope message)
        {
            if (_batching == null) throw new InvalidOperationException("This agent has not been started");

            return _serializing.SendAsync(message, _cancellation).ContinueWith(x =>
            {
                if (x.IsCompleted && !x.Result) Console.WriteLine("SendAsync rejected an outgoing message");
            });
        }

        public void Dispose()
        {
            _serializing?.Complete();
            _sender?.Complete();
            _batching?.Dispose();
        }
    }
}
