using System;
using System.Collections.Generic;
using Baseline;
using JasperBus.Queues;
using JasperBus.Runtime;

namespace JasperBus.Transports.LightningQueues
{
    public static class MessageExtensions
    {
        public static Envelope ToEnvelope(this Message message)
        {
            var envelope = new Envelope(message.Headers)
            {
                Data = message.Data
            };

            return envelope;
        }

        public static Message Copy(this Message message)
        {
            var copy = new Message
            {
                Data = message.Data,
                Headers = message.Headers,
            };

            return copy;
        }

        public static DateTime ExecutionTime(this Message message)
        {
            return message.ToEnvelope().ExecutionTime.Value;
        }

        public static void TranslateHeaders(this OutgoingMessage messagePayload)
        {
            string headerValue;
            messagePayload.Headers.TryGetValue(LightningQueuesTransport.MaxAttemptsHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.MaxAttempts = int.Parse(headerValue);
            }

            messagePayload.Headers.TryGetValue(LightningQueuesTransport.DeliverByHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.DeliverBy = DateTime.Parse(headerValue);
            }
        }

    }
}