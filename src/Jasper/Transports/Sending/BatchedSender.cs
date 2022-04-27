using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Logging;
using Jasper.Transports.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Sending
{
    public class BatchedSender : ISender, ISenderRequiresCallback
    {
        private readonly CancellationToken _cancellation;
        private readonly ILogger _logger;

        private readonly ISenderProtocol _protocol;
        private BatchingBlock<Envelope> _batching;
        private TransformBlock<Envelope?[], OutgoingMessageBatch> _batchWriting;
        private ISenderCallback _callback;
        private int _queued;
        private ActionBlock<OutgoingMessageBatch> _sender;
        private ActionBlock<Envelope?> _serializing;

        public BatchedSender(Uri? destination, ISenderProtocol protocol, CancellationToken cancellation, ILogger logger)
        {
            Destination = destination;
            _protocol = protocol;
            _cancellation = cancellation;
            _logger = logger;

            _sender = new ActionBlock<OutgoingMessageBatch>(SendBatch, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _cancellation,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });

            _sender.Completion.ContinueWith(x =>
            {
                if (x.IsFaulted) _logger.LogError(x.Exception, "Error trying to complete the sending block");
            }, _cancellation);

            _serializing = new ActionBlock<Envelope?>(async e =>
            {
                try
                {
                    await _batching.SendAsync(e);
                }
                catch (Exception? ex)
                {
                    _logger.LogError(ex, message: $"Error while trying to serialize envelope {e}");
                }
            },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation,
                    BoundedCapacity = DataflowBlockOptions.Unbounded
                });


            _serializing.Completion.ContinueWith(x =>
            {
                if (x.IsFaulted) _logger.LogError(x.Exception, "Failure while trying to shutdown serialization block");
            }, _cancellation);


            _batchWriting = new TransformBlock<Envelope?[], OutgoingMessageBatch>(
                envelopes =>
                {
                    var batch = new OutgoingMessageBatch(Destination, envelopes);
                    _queued += batch.Messages.Count;
                    return batch;
                },
                new ExecutionDataflowBlockOptions
                { BoundedCapacity = DataflowBlockOptions.Unbounded, MaxDegreeOfParallelism = 10, CancellationToken = _cancellation });

            _batchWriting.Completion.ContinueWith(x =>
            {
                if (x.IsFaulted) _logger.LogError(x.Exception, "Failure while trying to stop the batch writing block");
            }, _cancellation);

            _batchWriting.LinkTo(_sender);

            _batching = new BatchingBlock<Envelope>(200, _batchWriting, _cancellation);
            _batching.Completion.ContinueWith(x =>
            {
                if (x.IsFaulted) _logger.LogError(x.Exception, "Failure while trying to stop the batching block");
            }, _cancellation);
        }

        public Uri Destination { get; }

        public int QueuedCount => _queued + _batching.ItemCount + _serializing.InputCount;

        public bool Latched { get; private set; }

        public Task LatchAndDrain()
        {
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

            Latched = false;
        }

        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            var batch = OutgoingMessageBatch.ForPing(Destination);
            await _protocol.SendBatch(_callback, batch);

            return true;
        }

        public bool SupportsNativeScheduledSend { get; } = false;

        public ValueTask Send(Envelope message)
        {
            if (_batching == null) throw new InvalidOperationException("This agent has not been started");

            _serializing.Post(message);

            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            _serializing?.Complete();
            _sender?.Complete();
            _batching?.Dispose();
        }

        public void RegisterCallback(ISenderCallback senderCallback) => _callback = senderCallback;

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
            catch (Exception? e)
            {
                batchSendFailed(batch, e);
            }

            finally
            {
                _queued -= batch.Messages.Count;
            }
        }

        private void batchSendFailed(OutgoingMessageBatch batch, Exception? exception)
        {
            try
            {
                _callback.ProcessingFailure(batch, exception);
            }
            catch (Exception? e)
            {
                _logger.LogError(e, "Error while trying to process a send failure");
            }
        }
    }
}
