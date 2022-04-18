using Baseline.Dates;
using Jasper;
using Microsoft.Extensions.Hosting;

namespace DocumentationSamples
{

    public class PublishingSamples
    {
        public static async Task LocalQueuesApp()
                {
                    #region sample_LocalQueuesApp

                    using var host = await Host.CreateDefaultBuilder()
                        .UseJasper(opts =>
                        {
                            // Force a local queue to be
                            // strictly first in, first out
                            // with no more than a single
                            // thread handling messages enqueued
                            // here

                            // Use this option if message ordering is
                            // important
                            opts.LocalQueue("one")
                                .DurablyPersistedLocally()
                                .Sequential();

                            opts.LocalQueue("two")
                                .MaximumThreads(5);


                            // Or just edit the ActionBlock directly
                            opts.LocalQueue("three")
                                .ConfigureExecution(options =>
                                {
                                    options.MaxDegreeOfParallelism = 5;
                                    options.BoundedCapacity = 1000;
                                });
                        }).StartAsync();

                    #endregion
                }


        #region sample_CustomizingEnvelope
        public Task CustomizingEnvelope(IExecutionContext bus)
        {
            var envelope = new Envelope(new SomeMessage())
            {
                // Override the message routing by a Uri
                Destination = new Uri("tcp://server1:2000"),

                // Make this a scheduled message
                ScheduledTime = DateTime.UtcNow.AddDays(1),

                // Direct the receiver that the sender is interested in any response
                // with this message type

            };

            // This envelope should be discarded if not processed
            // successfully within 5 days
            envelope.DeliverBy = DateTimeOffset.UtcNow.AddDays(5);


            // Discard the message after 20 seconds if
            // not successfully processed before then
            envelope.DeliverWithin(20.Seconds());

            // Send the envelope and its contained message
            return bus.SendEnvelopeAsync(envelope);
            #endregion
        }

        #region sample_IServiceBus.Invoke
        public Task Invoke(IExecutionContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            return bus.InvokeAsync(@event);
        }
        #endregion

        #region sample_IServiceBus.Enqueue
        public Task Enqueue(IExecutionContext bus)
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
        #endregion

        #region sample_IServiceBus.Enqueue_to_specific_worker_queue
        public Task EnqueueToQueue(IExecutionContext bus)
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
        #endregion

        #region sample_send_delayed_message
        public async Task SendScheduledMessage(IExecutionContext bus, Guid invoiceId)
        {
            var message = new ValidateInvoiceIsNotLate
            {
                InvoiceId = invoiceId
            };

            // Schedule the message to be processed in a certain amount
            // of time
            await bus.ScheduleSendAsync(message, 30.Days());

            // Schedule the message to be processed at a certain time
            await bus.ScheduleSendAsync(message, DateTime.UtcNow.AddDays(30));
        }
        #endregion

        #region sample_schedule_job_locally
        public async Task ScheduleLocally(IExecutionContext bus, Guid invoiceId)
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
        #endregion

        #region sample_sending_message_with_servicebus
        public Task SendMessage(IExecutionContext bus)
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

            return bus.SendAsync(@event);
        }
        #endregion


        #region sample_publishing_message_with_servicebus
        public Task PublishMessage(IExecutionContext bus)
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

            return bus.PublishAsync(@event);
        }
        #endregion


        #region sample_send_message_to_specific_destination
        public async Task SendMessageToSpecificDestination(IExecutionContext bus)
        {
            var @event = new InvoiceCreated
            {
                Time = DateTime.UtcNow,
                Purchaser = "Guy Fieri",
                Amount = 112.34,
                Item = "Cookbook"
            };

            await bus.SendToDestinationAsync(new Uri("tcp://server1:2222"), @event);

            // or

            var envelope = new Envelope(@event)
            {
                Destination = new Uri("tcp://server1:2222")
            };

            await bus.SendEnvelopeAsync(envelope);
        }


        public class ValidateInvoiceIsNotLate
        {
            public Guid InvoiceId { get; set; }
        }
        #endregion

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



}
