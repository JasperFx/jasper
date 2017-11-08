using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.Util;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Transport
{
    public class HttpTransport : ITransport
    {
        private readonly HttpTransportSettings _settings;
        private readonly IWebHost _webHost;

        public HttpTransport(HttpTransportSettings settings, IWebHost webHost)
        {
            _settings = settings;
            _webHost = webHost;
        }


        public void Dispose()
        {

        }

        public string Protocol { get; } = "http";

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        // The sending agent will take care of this one
        public Uri LocalReplyUri { get; } = null;

        public void StartListening(BusSettings settings)
        {
            var addressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();
            var urls = addressesFeature?.Addresses ?? new string[0];

            //LocalReplyUri = _settings.LocalReplyUri.ToMachineUri() ?? urls.FirstOrDefault()?.ToUri().ToMachineUri();

        }

        public void Describe(TextWriter writer)
        {

        }
    }

    public class TransportEndpoint
    {
        public const string EnvelopeSenderHeader = "x-jasper-envelope-sender";

        // TODO -- may want to eventually have the URL be configurable
        public async Task<int> put__messages(HttpRequest request, IServiceBus bus, CompositeLogger logger)
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

        public async Task<int> put__messages_durable(HttpRequest request, IServiceBus bus, CompositeLogger logger)
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





    public class HttpSenderProtocol : ISenderProtocol, IDisposable
    {
        private readonly HttpClient _client;
        private readonly BusSettings _settings;

        public HttpSenderProtocol(BusSettings busSettings, HttpTransportSettings settings)
        {
            _client = new HttpClient
            {
                Timeout = settings.ConnectionTimeout
            };

            _settings = busSettings;
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
            request.Headers.Add(TransportEndpoint.EnvelopeSenderHeader, _settings.ServiceName);



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
