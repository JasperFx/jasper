using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Http.Transport
{
    public class HttpSenderProtocol : ISenderProtocol, IDisposable
    {
        private readonly HttpClient _client;
        private readonly MessagingSettings _settings;

        public HttpSenderProtocol(MessagingSettings settings, HttpTransportSettings httpSettings)
        {
            _client = new HttpClient
            {
                Timeout = httpSettings.ConnectionTimeout
            };

            _settings = settings;
        }

        public async Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch)
        {
            var bytes = batch.Data;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = batch.Destination,


            };

            //request.Headers.Add("content-length", bytes.Length.ToString());
            request.Headers.Add(TransportEndpoint.EnvelopeSenderHeader, _settings.ServiceName);

            request.Content = new ByteArrayContent(bytes);

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
                    await callback.TimedOut(batch);
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
