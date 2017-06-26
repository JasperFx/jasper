using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;

namespace Jasper.Bus.Transports.Lightweight
{
    public class BatchingBlock<T> : IDisposable
    {
        private readonly BatchBlock<T> _batchBlock;
        private readonly Timer _trigger;
        private readonly TransformBlock<T, T> _transformer;

        public BatchingBlock(int milliseconds, ITargetBlock<T[]> processor) : this(milliseconds.Milliseconds(), processor)
        {
        }

        public BatchingBlock(TimeSpan timeSpan, ITargetBlock<T[]> processor)
        {
            _batchBlock = new BatchBlock<T>(25);
            _trigger = new Timer(o => _batchBlock.TriggerBatch(), null, Timeout.Infinite, Timeout.Infinite);


            _transformer = new TransformBlock<T, T>(v =>
            {
                _trigger.Change(timeSpan, Timeout.InfiniteTimeSpan);
                return v;
            });

            _transformer.LinkTo(_batchBlock);

            _batchBlock.LinkTo(processor);
        }

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
