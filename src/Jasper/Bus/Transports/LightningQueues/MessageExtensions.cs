using System;
using Baseline;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.LightningQueues
{
    public static class MessageExtensions
    {
        public static Envelope ToEnvelope(this Envelope message)
        {
            var envelope = new Envelope(message.Headers)
            {
                Data = message.Data
            };

            return envelope;
        }

        public static Envelope Copy(this Envelope message)
        {
            var copy = new Envelope
            {
                Data = message.Data,
                Headers = message.Headers,
            };

            return copy;
        }

        public static DateTime ExecutionTime(this Envelope message)
        {
            return message.ToEnvelope().ExecutionTime.Value;
        }

        [Obsolete("ridiculous, get rid of this")]
        public static void TranslateHeaders(this Envelope messagePayload)
        {
            string headerValue;
            messagePayload.Headers.TryGetValue(Envelope.MaxAttemptsHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.MaxAttempts = int.Parse(headerValue);
            }

            messagePayload.Headers.TryGetValue(Envelope.DeliverByHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.DeliverBy = DateTime.Parse(headerValue);
            }
        }

    }
}
