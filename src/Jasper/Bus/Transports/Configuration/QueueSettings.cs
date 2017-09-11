using System;

namespace Jasper.Bus.Transports.Configuration
{
    public class QueueSettings : IQueueSettings
    {
        // Mostly for informative reasons
        public Uri Uri { get; set; }
        public string Name { get; }

        public QueueSettings(string name)
        {
            Name = name;
        }

        public int Parallelization { get; set; } = 5;

        IQueueSettings IQueueSettings.MaximumParallelization(int maximumParallelHandlers    )
        {
            Parallelization = maximumParallelHandlers;
            return this;
        }

        IQueueSettings IQueueSettings.Sequential()
        {
            Parallelization = 1;
            return this;
        }
    }
}