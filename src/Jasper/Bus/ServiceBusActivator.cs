using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus
{
    public class ServiceBusActivator
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly IDelayedJobProcessor _delayedJobs;
        private readonly ISubscriptionActivator _subscriptions;
        private readonly ITransport[] _transports;
        private readonly IUriLookup[] _lookups;

        public ServiceBusActivator(IHandlerPipeline pipeline, IDelayedJobProcessor delayedJobs, ISubscriptionActivator subscriptions, ITransport[] transports, IUriLookup[] lookups)
        {
            _pipeline = pipeline;
            _delayedJobs = delayedJobs;
            _subscriptions = subscriptions;
            _transports = transports;
            _lookups = lookups;
        }
    }
}
