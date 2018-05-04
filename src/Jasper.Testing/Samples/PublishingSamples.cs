using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;

namespace Jasper.Testing.Samples
{
    public class PublishingSamples
    {

        // SAMPLE: CustomizingEnvelope
        public Task CustomizingEnvelope(IMessageContext bus)
        {
            return bus.Send(new SomeMessage(), e =>
            {
                // Override the message routing
                e.Destination = new Uri("tcp://server1:2000");

                // Force Jasper to send the message serialized
                // with a certain content-type
                e.ContentType = "application/xml";


                // Make this a scheduled job
                e.ExecutionTime = DateTime.UtcNow.AddDays(1);


                // Direct the receiver that the sender is interested in any response
                // with this message type
                e.ReplyRequested = "other-message-type";

                // Probably easier to use this extension method though
                e.ReplyRequested = typeof(Message2).ToMessageAlias();
            });
            // ENDSAMPLE
        }



        public class ValidateInvoiceIsNotLate
        {
            public Guid InvoiceId { get; set; }
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

        // SAMPLE: sending-message-with-send-and-await
        public async Task SendMessageAndAwait(IMessageContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            await bus.SendAndWait(@event);
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

            await bus.Send(new Uri("tcp://server1:2222"), @event);

            // or

            await bus.Send(@event, e =>
            {
                e.Destination = new Uri("tcp://server1:2222");
            });
        }
        // ENDSAMPLE

        public class InvoiceCreated
        {
            public DateTime Time { get; set; }
            public string Purchaser { get; set; }
            public double Amount { get; set; }
            public string Item { get; set; }
        }

        public class SomeMessage{}

    }


}
