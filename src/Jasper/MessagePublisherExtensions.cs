using System;
using System.Threading.Tasks;

namespace Jasper
{
    public static class MessagePublisherExtensions
    {
        /// <summary>
        ///     Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        public static Task Send<T>(this IMessagePublisher publisher, Uri destination, T message)
        {
            return publisher.SendEnvelope(new Envelope {Message = message, Destination = destination});
        }

        /// <summary>
        ///     Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        public static Task ScheduleSend<T>(this IMessagePublisher publisher, T message, DateTime time)
        {
            return publisher.SendEnvelope(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime(),
                Status = EnvelopeStatus.Scheduled
            });
        }

        /// <summary>
        ///     Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        public static Task ScheduleSend<T>(this IMessagePublisher publisher, T message, TimeSpan delay)
        {
            return publisher.ScheduleSend(message, DateTime.UtcNow.Add(delay));
        }

    }
}
