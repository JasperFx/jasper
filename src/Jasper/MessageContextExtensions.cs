using System;
using System.Threading.Tasks;

namespace Jasper
{
    public static class MessageContextExtensions
    {

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
