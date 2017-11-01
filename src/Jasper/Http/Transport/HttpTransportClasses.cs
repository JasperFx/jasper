using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Transport
{
    public class HttpTransport : ITransport
    {
        private readonly HttpTransportSettings _settings;

        public HttpTransport(HttpTransportSettings settings)
        {
            _settings = settings;

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // TODO -- what if it really needs to be 'https'?
        public string Protocol { get; } = "http";
        public Task Send(Envelope envelope, Uri destination)
        {
            throw new NotImplementedException();
        }

        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            throw new NotImplementedException();
        }

        public Uri DefaultReplyUri()
        {
            throw new NotImplementedException();
        }

        public TransportState State => _settings.State;

        public void Describe(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class TransportEndpoint
    {
        public const string EnvelopeSenderHeader = "x-jasper-envelope-sender";

        // TODO -- may want to eventually have the URL be configurable
        public async Task<int> put__transport(HttpRequest request, IServiceBus bus, CompositeLogger logger)
        {
            try
            {
                // TODO -- optimize the reading here to reduce allocations
                var bytes = await request.Body.ReadBytesAsync(request.ContentLength);
                var envelopes = Envelope.ReadMany(bytes);

                foreach (var envelope in envelopes)
                {
                    await bus.Enqueue(envelope);
                }

                return 200;
            }
            catch (Exception e)
            {
                var message = $"Error receiving envelopes from {request.Headers["x-jasper-envelope-sender"]}";
                logger.LogException(e, message:message);

                return 500;
            }
        }
    }

    public class HttpTransportSettings
    {
        public TimeSpan ConnectionTimeout { get; set; } = 10.Seconds();
        public string RelativeUrl { get; set; } = "_transport";

        // TODO -- might need a 3rd state for "send only"
        public TransportState State { get; set; } = TransportState.Disabled;
    }



    public class HttpSenderProtocol : ISenderProtocol, IDisposable
    {
        private readonly HttpClient _client;
        private readonly JasperRuntime _runtime;

        public HttpSenderProtocol(JasperRuntime runtime, HttpTransportSettings settings)
        {
            _client = new HttpClient
            {
                Timeout = settings.ConnectionTimeout
            };

            _runtime = runtime;
        }

        public async Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch)
        {
            // TODO -- optimize the reading here to reduce allocations
            var bytes = Envelope.Serialize(batch.Messages);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = batch.Destination,
                Content = new ByteArrayContent(bytes),

            };

            request.Headers.Add("content-length", bytes.Length.ToString());
            request.Headers.Add(TransportEndpoint.EnvelopeSenderHeader, _runtime.ServiceName);



            // TODO -- security here?



            try
            {
                var response = await _client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Unable to send message batch to " + batch.Destination);
                }

            }
            catch (TaskCanceledException e)
            {
                if (!e.CancellationToken.IsCancellationRequested)
                {
                    callback.TimedOut(batch);
                }
                else
                {
                    throw;
                }
            }

        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
