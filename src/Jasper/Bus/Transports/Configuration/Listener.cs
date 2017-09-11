using System;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Transports.Configuration
{
    public class Listener : IQueueSettings
    {
        public Uri Uri { get; private set; }

        public Listener(Uri uri)
        {
            Uri = uri;
        }

        public int MaximumParallelization { get; set; } = 5;

        IQueueSettings IQueueSettings.MaximumParallelization(int maximumParallelHandlers)
        {
            MaximumParallelization = maximumParallelHandlers;
            return this;
        }

        IQueueSettings IQueueSettings.Sequential()
        {
            MaximumParallelization = 1;
            return this;
        }

        public void ReadAlias(UriAliasLookup lookups)
        {
            Uri = lookups.Resolve(Uri);
        }
    }
}
