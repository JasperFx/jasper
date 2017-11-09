using System;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Transport
{
    public class TransportEndpoint
    {
        public const string EnvelopeSenderHeader = "x-jasper-envelope-sender";

        // TODO -- may want to eventually have the URL be configurable
        public async Task<int> put__messages(HttpRequest request, ILocalWorkerSender workers, CompositeLogger logger)
        {
            try
            {
                // TODO -- optimize the reading here to reduce allocations
                var bytes = await request.Body.ReadBytesAsync(request.ContentLength);
                var envelopes = Envelope.ReadMany(bytes);

                await workers.EnqueueLightweight(envelopes);

                return 200;
            }
            catch (Exception e)
            {
                var message = $"Error receiving envelopes from {request.Headers["x-jasper-envelope-sender"]}";
                logger.LogException(e, message:message);

                return 500;
            }
        }

        public async Task<int> put__messages_durable(HttpRequest request, ILocalWorkerSender workers, CompositeLogger logger)
        {
            try
            {
                // TODO -- optimize the reading here to reduce allocations
                var bytes = await request.Body.ReadBytesAsync(request.ContentLength);
                var envelopes = Envelope.ReadMany(bytes);

                await workers.EnqueueDurably(envelopes);

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
}
