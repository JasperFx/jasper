using System;
using Baseline;

namespace JasperBus.Transports.LightningQueues
{
    public class LightningQueueSettings 
    {
        /// <summary>
        /// The number of databases (queues) allowed for the lmdb storage, default is 5
        /// </summary>
        public int MaxDatabases { get; set; } = 5;

        /// <summary>
        /// The maximum map size in bytes for the underlying lmdb storage, default is 100 MB in bytes
        /// </summary>
        public int MapSize { get; set; } = 1024*1024*100;

        public string QueuePath { get; set; } = AppContext.BaseDirectory.AppendPath("jasperqueues");

        public Uri DefaultReplyUri { get; set; } = new Uri("lq.tcp://localhost:2345/replies");
    }
}