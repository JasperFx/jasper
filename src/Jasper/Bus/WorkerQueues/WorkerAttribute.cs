using System;

namespace Jasper.Bus.WorkerQueues
{
    /// <summary>
    /// Directs Jasper to process this message type in the named
    /// worker queue
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class WorkerAttribute : Attribute
    {
        public string WorkerName { get; }


        public WorkerAttribute(string workerName)
        {
            WorkerName = workerName.ToLower();
        }

        /// <summary>
        /// If set, tells Jasper to use this number as the maximum
        /// number of concurrent threads handling this named worker queue
        /// </summary>
        public int MaximumParallelization { get; set; }

        /// <summary>
        /// Should this message be enqueued in the durable
        /// worker on calls to IServiceBus.Enqueue()
        /// </summary>
        public bool IsDurable { get; set; }
    }
}