using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;

namespace Jasper.Bus.Transports.Util
{
    public class BatchingBlock<T> : IDisposable
    {
        private readonly BatchBlock<T> _batchBlock;
        private readonly Timer _trigger;
        private readonly TransformBlock<T, T> _transformer;

        public BatchingBlock(int milliseconds, ITargetBlock<T[]> processor,
            CancellationToken cancellation = default(CancellationToken))
        : this(milliseconds.Milliseconds(), processor, cancellation)
        {

        }

        public BatchingBlock(TimeSpan timeSpan, ITargetBlock<T[]> processor, CancellationToken cancellation = default(CancellationToken))
        {
            _batchBlock = new BatchBlock<T>(25, new GroupingDataflowBlockOptions
            {
                CancellationToken = cancellation
            });

            _trigger = new Timer(o => _batchBlock.TriggerBatch(), null, Timeout.Infinite, Timeout.Infinite);


            _transformer = new TransformBlock<T, T>(v =>
            {
                _trigger.Change(timeSpan, Timeout.InfiniteTimeSpan);
                return v;
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellation
            });

            _transformer.LinkTo(_batchBlock);

            _batchBlock.LinkTo(processor);
        }

        public int ItemCount => _batchBlock.OutputCount;

        public void Post(T item)
        {
            _transformer.Post(item);
        }


        public void Dispose()
        {
            _trigger?.Dispose();
            _batchBlock?.Complete();
            _transformer?.Complete();
        }
    }
}
