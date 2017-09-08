using System;
using Baseline;
using Jasper.Bus.Runtime;
using LightningDB;

namespace Jasper.LightningDb
{
    public class LightningDbSettings
    {
        public string QueuePath { get; set; } = AppContext.BaseDirectory.AppendPath("jasperqueues");

        /// <summary>
        /// The number of databases (queues) allowed for the lmdb storage, default is 5
        /// </summary>
        public int MaxDatabases { get; set; } = 5;

        /// <summary>
        /// The maximum map size in bytes for the underlying lmdb storage, default is 100 MB in bytes
        /// </summary>
        public int MapSize { get; set; } = 1024*1024*100;

        public int MaximumSendAttempts { get; set; } = 100;

        public LightningEnvironment ToEnvironment()
        {
            return new LightningEnvironment(QueuePath, new EnvironmentConfiguration
            {
                MapSize = MapSize,
                MaxDatabases = MaxDatabases
            });
        }
    }
}
