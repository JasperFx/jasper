using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;

namespace Jasper.Messaging.Transports.Util
{
    public class BatchingBlock<T> : IDisposable
    {
        private readonly TimeSpan _timeSpan;
        private readonly BatchBlock<T> _batchBlock;
        private readonly Timer _trigger;

        public BatchingBlock(int milliseconds, ITargetBlock<T[]> processor,
            CancellationToken cancellation = default(CancellationToken))
        : this(milliseconds.Milliseconds(), processor, cancellation)
        {

        }

        public BatchingBlock(TimeSpan timeSpan, ITargetBlock<T[]> processor, CancellationToken cancellation = default(CancellationToken))
        {
            _timeSpan = timeSpan;
            _batchBlock = new BatchBlock<T>(25, new GroupingDataflowBlockOptions
            {
                CancellationToken = cancellation
            });

            _batchBlock.Completion.ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    Console.WriteLine(x.Exception);
                }
            });

            _trigger = new Timer(o => {
                try
                {
                    _batchBlock.TriggerBatch();
                }
                catch (Exception)
                {
                    // ignored
                }
            }, null, Timeout.Infinite, Timeout.Infinite);



            _batchBlock.LinkTo(processor);
        }

        public int ItemCount => _batchBlock.OutputCount;

        public void Post(T item)
        {
            try
            {
                _trigger.Change(_timeSpan, Timeout.InfiniteTimeSpan);
            }
            catch (Exception)
            {
                // ignored
            }

            _batchBlock.Post(item);
        }

        public void Complete()
        {
            _batchBlock.Complete();
        }

        public Task Completion => _batchBlock.Completion;


        public void Dispose()
        {
            _trigger?.Dispose();
            _batchBlock?.Complete();
        }
    }
}
