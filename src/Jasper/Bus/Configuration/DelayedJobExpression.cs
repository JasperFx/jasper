using System;
using Jasper.Bus.Delayed;

namespace Jasper.Bus.Configuration
{
    [Obsolete("Doesn't really make sense to even bother with this when it's so rarely used")]
    public class DelayedJobExpression
    {
        private readonly ServiceBusFeature _feature;

        public DelayedJobExpression(ServiceBusFeature feature)
        {
            _feature = feature;
        }

        public void RunInMemory()
        {
            _feature.DelayedJobsRunInMemory = true;
        }

        public void Use<T>() where T : class, IDelayedJobProcessor
        {
            _feature.DelayedJobsRunInMemory = false;
            _feature.Services.ForSingletonOf<IDelayedJobProcessor>().Use<T>();
        }

        public void Use(IDelayedJobProcessor delayedJobs)
        {
            _feature.DelayedJobsRunInMemory = false;
            _feature.Services.ForSingletonOf<IDelayedJobProcessor>().Use(delayedJobs);
        }
    }
}
