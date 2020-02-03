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

        /// <summary>
        /// Send a response message back to the original sender of the message being handled.
        /// This can only be used from within a message handler
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Task RespondToSender(this IMessageContext context, object response)
        {
            if (context.Envelope == null) throw new InvalidOperationException("This operation can only be performed while in the middle of handling an incoming message");

            return context.SendToDestination(context.Envelope.ReplyUri, response);
        }

    }
}
