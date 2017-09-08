using System;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Util;

namespace Jasper.Bus.Logging
{
    public class ConsoleBusLogger : IBusLogger
    {
        public void Sent(Envelope envelope)
        {
            Console.WriteLine($"Sent {envelope.Message.GetType().Name}#{envelope.CorrelationId} to {envelope.Destination}");
        }

        public void Received(Envelope envelope)
        {
            Console.WriteLine($"Received {envelope.Message?.GetType().Name}#{envelope.CorrelationId} at {envelope.Destination} from {envelope.ReplyUri}");
        }

        public void ExecutionStarted(Envelope envelope)
        {
            Console.WriteLine($"Started processing {envelope.Message?.GetType().Name}#{envelope.CorrelationId}");
        }

        public void ExecutionFinished(Envelope envelope)
        {
            Console.WriteLine($"Finished processing {envelope.Message?.GetType().Name}#{envelope.CorrelationId}");
        }

        public void MessageSucceeded(Envelope envelope)
        {
            ConsoleWriter.Write(ConsoleColor.Green, $"Successfully processed message {envelope.Message?.GetType().Name}#{envelope.CorrelationId}");
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            ConsoleWriter.Write(ConsoleColor.Red, $"Failed to process message {envelope.Message?.GetType().Name}#{envelope.CorrelationId}");
            ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
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
            ConsoleWriter.Write(ConsoleColor.Yellow, $"No known handler for {envelope.Message?.GetType().Name}#{envelope.CorrelationId} from {envelope.ReplyUri}");
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
    }
}
