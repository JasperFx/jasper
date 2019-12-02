namespace Jasper.Configuration
{
    public class SubscriberConfiguration : ISubscriberConfiguration
    {
        private readonly Endpoint _endpoint;

        public SubscriberConfiguration(Endpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public ISubscriberConfiguration Durably()
        {
            _endpoint.IsDurable = true;
            return this;
        }

        public ISubscriberConfiguration Lightweight()
        {
            _endpoint.IsDurable = false;
            return this;
        }
    }
}