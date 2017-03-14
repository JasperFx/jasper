using System;
using System.Threading.Tasks;

namespace JasperBus
{
    public class ServiceBus : IServiceBus
    {
        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(T message)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(Uri destination, T message)
        {
            throw new NotImplementedException();
        }

        public void Consume<T>(T message)
        {
            throw new NotImplementedException();
        }

        public void DelaySend<T>(T message, DateTime time)
        {
            throw new NotImplementedException();
        }

        public void DelaySend<T>(T message, TimeSpan delay)
        {
            throw new NotImplementedException();
        }

        public Task SendAndWait<T>(T message)
        {
            throw new NotImplementedException();
        }

        public Task SendAndWait<T>(Uri destination, T message)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSubscriptionsForThisNodeAsync()
        {
            throw new NotImplementedException();
        }
    }
}