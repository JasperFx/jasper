using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Util;
using TestMessages;

namespace Jasper.Testing.Samples
{
    public class PublishingSamples
    {
        // SAMPLE: CustomizingEnvelope
        public Task CustomizingEnvelope(IMessageContext bus)
        {
            var envelope = new Envelope(new SomeMessage())
            {
                // Override the message routing by a Uri
                Destination = new Uri("tcp://server1:2000"),

                // Force Jasper to send the message serialized
                // with a certain content-type
                ContentType = "application/xml",

                // Make this a scheduled message
                ExecutionTime = DateTime.UtcNow.AddDays(1),

                // Direct the receiver that the sender is interested in any response
                // with this message type

                // Probably easier to use this extension method though
                ReplyRequested = "other-message-type"
            };

            // This envelope should be discarded if not processed
            // successfully within 5 days
            envelope.DeliverBy = DateTimeOffset.UtcNow.AddDays(5);


            // Discard the message after 20 seconds if
            // not successfully processed before then
            envelope.DeliverWithin(20.Seconds());

            // Send the envelope and its contained message
            return bus.SendEnvelope(envelope);
            // ENDSAMPLE
        }

        // SAMPLE: IServiceBus.Invoke
        public Task Invoke(IMessageContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            return bus.Invoke(@event);
        }
        // ENDSAMPLE

        // SAMPLE: IServiceBus.Enqueue
        public Task Enqueue(IMessageContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            return bus.Enqueue(@event);
        }
        // ENDSAMPLE

        // SAMPLE: IServiceBus.Enqueue-to-specific-worker-queue
        public Task EnqueueToQueue(IMessageContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            // Put this message in a local worker
            // queue named 'highpriority'
            return bus.Enqueue(@event, "highpriority");
        }
        // ENDSAMPLE

        // SAMPLE: send-delayed-message
        public async Task SendScheduledMessage(IMessageContext bus, Guid invoiceId)
        {
            var message = new ValidateInvoiceIsNotLate
            {
                InvoiceId = invoiceId
            };

            // Schedule the message to be processed in a certain amount
            // of time
            await bus.ScheduleSend(message, 30.Days());

            // Schedule the message to be processed at a certain time
            await bus.ScheduleSend(message, DateTime.UtcNow.AddDays(30));
        }
        // ENDSAMPLE

        // SAMPLE: schedule-job-locally
        public async Task ScheduleLocally(IMessageContext bus, Guid invoiceId)
        {
            var message = new ValidateInvoiceIsNotLate
            {
                InvoiceId = invoiceId
            };

            // Schedule the message to be processed in a certain amount
            // of time
            await bus.Schedule(message, 30.Days());

            // Schedule the message to be processed at a certain time
            await bus.Schedule(message, DateTime.UtcNow.AddDays(30));
        }
        // ENDSAMPLE

        // SAMPLE: sending-message-with-servicebus
        public Task SendMessage(IMessageContext bus)
        {
            // In this case, we're sending an "InvoiceCreated"
            // message
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            return bus.Send(@event);
        }
        // ENDSAMPLE


        // SAMPLE: publishing-message-with-servicebus
        public Task PublishMessage(IMessageContext bus)
        {
            // In this case, we're sending an "InvoiceCreated"
            // message
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            return bus.Publish(@event);
        }
        // ENDSAMPLE


        // SAMPLE: send-message-to-specific-destination
        public async Task SendMessageToSpecificDestination(IMessageContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            await bus.SendToDestination(new Uri("tcp://server1:2222"), @event);

            // or

            var envelope = new Envelope(@event)
            {
                Destination = new Uri("tcp://server1:2222")
            };

            await bus.SendEnvelope(envelope);
        }


        public class ValidateInvoiceIsNotLate
        {
            public Guid InvoiceId { get; set; }
        }
        // ENDSAMPLE

        public class InvoiceCreated
        {
            public DateTime Time { get; set; }
            public string Purchaser { get; set; }
            public double Amount { get; set; }
            public string Item { get; set; }
        }

        public class SomeMessage
        {
        }
    }

    // SAMPLE: LocalQueuesApp
    public class LocalQueuesApp : JasperOptions
    {
        public LocalQueuesApp()
        {
            // Force a local queue to be
            // strictly first in, first out
            // with no more than a single
            // thread handling messages enqueued
            // here

            // Use this option if message ordering is
            // important
            Endpoints
                .LocalQueue("one")
                .Durable()
                .Sequential();


            Endpoints
                .LocalQueue("two")
                .MaximumThreads(5);


            // Or just edit the ActionBlock directly
            Endpoints.LocalQueue("three")
                .ConfigureExecution(options =>
                {
                    options.MaxDegreeOfParallelism = 5;
                    options.BoundedCapacity = 1000;
                });
        }
    }
    // ENDSAMPLE
}
