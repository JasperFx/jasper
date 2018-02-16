using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Util;

namespace Jasper.Messaging.Logging
{
    public class ConsoleMessageSink : IMessageEventSink
    {
        public void Sent(Envelope envelope)
        {
            Console.WriteLine($"Sent {envelope.Message.GetType().Name}#{envelope.Id} to {envelope.Destination}");
        }

        public void Received(Envelope envelope)
        {
            Console.WriteLine($"Received {envelope.Message?.GetType().Name}#{envelope.Id} at {envelope.Destination} from {envelope.ReplyUri}");
        }

        public void ExecutionStarted(Envelope envelope)
        {
            Console.WriteLine($"Started processing {envelope.Message?.GetType().Name}#{envelope.Id}");
        }

        public void ExecutionFinished(Envelope envelope)
        {
            Console.WriteLine($"Finished processing {envelope.Message?.GetType().Name}#{envelope.Id}");
        }

        public void MessageSucceeded(Envelope envelope)
        {
            ConsoleWriter.Write(ConsoleColor.Green, $"Successfully processed message {envelope.Message?.GetType().Name}#{envelope.Id}");
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            ConsoleWriter.Write(ConsoleColor.Red, $"Failed to process message {envelope.Message?.GetType().Name}#{envelope.Id}");
            ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }

        public void DiscardedEnvelope(Envelope envelope)
        {
            Console.WriteLine($"Expired envelope {envelope} was discarded");
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {

            ConsoleWriter.Write(ConsoleColor.Red, message);

            if (correlationId.IsNotEmpty())
            {
                ConsoleWriter.Write(ConsoleColor.Red, $"Id: {correlationId}");
            }

            ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }

        public void NoHandlerFor(Envelope envelope)
        {
            ConsoleWriter.Write(ConsoleColor.Yellow, $"No known handler for {envelope.Message?.GetType().Name}#{envelope.Id} from {envelope.ReplyUri}");
        }

        public void NoRoutesFor(Envelope envelope)
        {
            ConsoleWriter.Write(ConsoleColor.Yellow, $"No routes can be determined for {envelope}");
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            ConsoleWriter.Write(ConsoleColor.Yellow, $"Subscriber mismatch: {mismatch}");
        }

        public void Undeliverable(Envelope envelope)
        {
            ConsoleWriter.Write(ConsoleColor.Red, $"Could not deliver {envelope}");
        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            ConsoleWriter.Write(ConsoleColor.Yellow, $"Envelope {envelope} was moved to the error queue with exception:");
        }
    }
}
