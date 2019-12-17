using LamarCodeGeneration.Util;

namespace Jasper.Configuration
{
    public class SubscriberConfiguration<T, TEndpoint> : ISubscriberConfiguration<T> where TEndpoint : Endpoint where T : ISubscriberConfiguration<T>
    {
        protected readonly TEndpoint _endpoint;

        public SubscriberConfiguration(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        protected TEndpoint Endpoint => _endpoint;

        public T Durably()
        {
            _endpoint.IsDurable = true;
            return this.As<T>();
        }

        public T Lightweight()
        {
            _endpoint.IsDurable = false;
            return this.As<T>();
        }
    }

    public class SubscriberConfiguration : SubscriberConfiguration<ISubscriberConfiguration, Endpoint>, ISubscriberConfiguration
    {
        public SubscriberConfiguration(Endpoint endpoint) : base(endpoint)
        {
        }
    }
}
