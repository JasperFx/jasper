using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Http.Transport
{
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