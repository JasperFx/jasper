using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Transport
{
    public class TransportEndpoint
    {
        public const string EnvelopeSenderHeader = "x-jasper-envelope-sender";

        public async Task<int> put__messages(HttpRequest request, ILocalWorkerSender workers, CompositeMessageLogger logger)
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
                logger.LogException(e, message: message);

                return 500;
            }
        }

        public async Task<int> put__messages_durable(HttpRequest request, ILocalWorkerSender workers, CompositeMessageLogger logger)
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
                logger.LogException(e, message: message);

                return 500;
            }
        }
    }
}
