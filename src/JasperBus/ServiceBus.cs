using System;
using System.Threading.Tasks;
using Baseline.Dates;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IReplyWatcher _watcher;
        private readonly IHandlerPipeline _pipeline;

        public ServiceBus(IEnvelopeSender sender, IReplyWatcher watcher, IHandlerPipeline pipeline)
        {
            _sender = sender;
            _watcher = watcher;
            _pipeline = pipeline;
        }

        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            options = options ?? new RequestOptions();

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = typeof(TResponse).Name
            };

            if (options.Destination != null)
            {
                envelope.Destination = options.Destination;
            }


            var task = _watcher.StartWatch<TResponse>(envelope.CorrelationId, options.Timeout);

            _sender.Send(envelope);

            return task;
        }

        public void Send<T>(T message)
        {
            _sender.Send(new Envelope {Message = message});
        }

        public void Send<T>(Uri destination, T message)
        {
            _sender.Send(new Envelope { Message = message, Destination = destination});
        }

        public Task Consume<T>(T message)
        {
            return _pipeline.InvokeNow(message);
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
            return GetSendAndWaitTask(message);
        }

        public Task SendAndWait<T>(Uri destination, T message)
        {
            return GetSendAndWaitTask(message, destination);
        }

        private Task GetSendAndWaitTask<T>(T message, Uri destination = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true,
                Destination = destination
            };

            var task = _watcher.StartWatch<Acknowledgement>(envelope.CorrelationId, 10.Minutes());


            _sender.Send(envelope);

            return task;
        }

    }
}