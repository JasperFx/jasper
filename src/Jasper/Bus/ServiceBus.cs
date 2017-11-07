using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IReplyWatcher _watcher;
        private readonly IHandlerPipeline _pipeline;
        private readonly SerializationGraph _serialization;

        public ServiceBus(IEnvelopeSender sender, IReplyWatcher watcher, IHandlerPipeline pipeline, BusMessageSerializationGraph serialization)
        {
            _sender = sender;
            _watcher = watcher;
            _pipeline = pipeline;
            _serialization = serialization;
        }

        public async Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            options = options ?? new RequestOptions();

            var envelope = EnvelopeForRequestResponse<TResponse>(request);

            if (options.Destination != null)
            {
                envelope.Destination = options.Destination;
            }


            var watcher = _watcher.StartWatch<TResponse>(envelope.Id, options.Timeout);

            await _sender.Send(envelope);

            return await watcher;
        }

        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageAlias();
            var reader = _serialization.ReaderFor(messageType);

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes

            };
            return envelope;
        }

        public Task Send<T>(T message)
        {
            return _sender.Send(new Envelope {Message = message});
        }

        public Task Send<T>(T message, Action<Envelope> customize)
        {
            var envelope = new Envelope {Message = message};
            customize(envelope);

            return _sender.Send(envelope);
        }

        public Task Send<T>(Uri destination, T message)
        {
            return _sender.Send(new Envelope { Message = message, Destination = destination});
        }

        public Task Invoke<T>(T message)
        {
            return _pipeline.InvokeNow(new Envelope(message)
            {
                Callback = new InvocationCallback(),
                ReplyUri = TransportConstants.RepliesUri
            });
        }

        private class InvocationCallback : IMessageCallback
        {
            public Task MarkSuccessful()
            {
                return Task.CompletedTask;
            }

            public Task MarkFailed(Exception ex)
            {
                return Task.CompletedTask;
            }

            public Task MoveToErrors(ErrorReport report)
            {
                return Task.CompletedTask;
            }

            public Task Requeue(Envelope envelope)
            {
                return Task.CompletedTask;
            }

            public Task MoveToDelayedUntil(DateTime time, Envelope envelope)
            {
                return Task.CompletedTask;
            }
        }

        public Task Enqueue<T>(T message)
        {
            return _sender.EnqueueLocally(message);
        }

        public Task DelaySend<T>(T message, DateTime time)
        {
            return _sender.Send(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
            });
        }

        public Task DelaySend<T>(T message, TimeSpan delay)
        {
            return DelaySend(message, DateTime.UtcNow.Add(delay));
        }

        public Task SendAndWait<T>(T message)
        {
            return GetSendAndWaitTask(message);
        }

        public Task SendAndWait<T>(Uri destination, T message)
        {
            return GetSendAndWaitTask(message, destination);
        }

        private async Task GetSendAndWaitTask<T>(T message, Uri destination = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true,
                Destination = destination
            };

            var task = _watcher.StartWatch<Acknowledgement>(envelope.Id, 10.Minutes());


            await _sender.Send(envelope);

            await task;
        }

    }
}
