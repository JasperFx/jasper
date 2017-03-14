using System;
using System.Threading.Tasks;
using JasperBus.Runtime;

namespace JasperBus
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;

        public ServiceBus(IEnvelopeSender sender)
        {
            _sender = sender;
        }

        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(T message)
        {
            _sender.Send(new Envelope {Message = message});
        }

        public void Send<T>(Uri destination, T message)
        {
            _sender.Send(new Envelope { Message = message, Destination = destination});
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