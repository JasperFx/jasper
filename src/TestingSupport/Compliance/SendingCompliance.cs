using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Serialization;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton.Resources;
using Shouldly;
using TestingSupport.ErrorHandling;
using TestMessages;
using Xunit;


namespace TestingSupport.Compliance
{
    public abstract class SendingComplianceFixture : IDisposable
    {
        public IHost Sender { get; private set; }
        public IHost Receiver { get; private set; }
        public Uri OutboundAddress { get; protected set; }

        public readonly TimeSpan DefaultTimeout = 5.Seconds();

        protected SendingComplianceFixture(Uri destination, int defaultTimeInSeconds = 5)
        {
            OutboundAddress = destination;
            DefaultTimeout = defaultTimeInSeconds.Seconds();
        }


        protected Task TheOnlyAppIs(Action<JasperOptions> configure)
        {
            AllLocally = true;

            Sender = JasperHost.For(options =>
            {
                configure(options);
                configureReceiver(options);
                configureSender(options);
            });

            return Task.CompletedTask;
        }

        public bool AllLocally { get; set; }

        protected async Task SenderIs(Action<JasperOptions> configure)
        {
            Sender = JasperHost.For(opts =>
            {
                configure(opts);
                configureSender(opts);
            });

        }

        private void configureSender(JasperOptions options)
        {
            options.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<PongHandler>();

            options.AddSerializer(new GreenTextWriter());
            options.ServiceName = "SenderService";
            options.PublishAllMessages().To(OutboundAddress);

            options.Services.AddSingleton<IMessageSerializer, GreenTextWriter>();
            options.Services.AddResourceSetupOnStartup(StartupAction.ResetState);
        }

        public async Task ReceiverIs(Action<JasperOptions> configure)
        {
            Receiver = JasperHost.For(opts =>
            {
                configure(opts);
                configureReceiver(opts);
            });
        }

        private static void configureReceiver(JasperOptions options)
        {
            options.Handlers.Retries.MaximumAttempts = 3;
            options.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>()
                .IncludeType<ExecutedMessageGuy>()
                .IncludeType<ColorHandler>()
                .IncludeType<ErrorCausingMessageHandler>()
                .IncludeType<BlueHandler>()
                .IncludeType<PingHandler>();

            options.AddSerializer(new BlueTextReader());

            options.Handlers.OnException<DivideByZeroException>()
                .MoveToErrorQueue();

            options.Handlers.OnException<DataMisalignedException>()
                .Requeue(3);

            options.Handlers.OnException<BadImageFormatException>()
                .RetryLater(3.Seconds());


            options.Services.AddSingleton(new ColorHistory());

            options.Services.AddResourceSetupOnStartup(StartupAction.ResetState);
        }

        public void Dispose()
        {
            Sender?.Dispose();
            if (!object.ReferenceEquals(Sender, Receiver))
            {
                Receiver?.Dispose();
            }
        }

        public virtual void BeforeEach(){}
    }

    public abstract class SendingCompliance<T> : IAsyncLifetime where T : SendingComplianceFixture, new()
    {
        protected IHost theSender;
        protected IHost theReceiver;
        protected Uri theOutboundAddress;


        protected readonly ErrorCausingMessage theMessage = new ErrorCausingMessage();
        private ITrackedSession _session;

        protected SendingCompliance()
        {
            Fixture = new T();

        }

        public async Task InitializeAsync()
        {
            if (Fixture is IAsyncLifetime lifetime)
            {
                await lifetime.InitializeAsync();
            }

            theSender = Fixture.Sender;
            theReceiver = Fixture.Receiver;
            theOutboundAddress = Fixture.OutboundAddress;

            await Fixture.Sender.ResetResourceState();

            if (Fixture.Receiver != null && !object.ReferenceEquals(Fixture.Sender, Fixture.Receiver))
            {
                await Fixture.Receiver.ResetResourceState();
            }

            Fixture.BeforeEach();
        }

        public Task DisposeAsync()
        {
            Fixture.SafeDispose();
            return Task.CompletedTask;
        }

        public T Fixture { get; }

        [Fact]
        public virtual async Task can_apply_requeue_mechanics()
        {
            var session = await theSender.TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .Timeout(15.Seconds())
                .ExecuteAndWaitAsync(c => c.SendToDestinationAsync(theOutboundAddress, new Message2()));

            session.FindSingleTrackedMessageOfType<Message2>(EventType.MessageSucceeded)
                .ShouldNotBeNull();

        }

        [Fact]
        public async Task can_send_from_one_node_to_another_by_destination()
        {
            var session = await theSender.TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWaitAsync(c => c.SendToDestinationAsync(theOutboundAddress, new Message1()));


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .ShouldNotBeNull();
        }

        [Fact]
        public async Task can_send_from_one_node_to_another_by_publishing_rule()
        {
            var message1 = new Message1();

            var session = await theSender.TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .Timeout(30.Seconds())
                .SendMessageAndWaitAsync(message1);


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .Id.ShouldBe(message1.Id);
        }

        [Fact]
        public async Task tags_the_envelope_with_the_source()
        {
            var session = await theSender.TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWaitAsync(c => c.SendToDestinationAsync(theOutboundAddress, new Message1()));


            var record = session.FindEnvelopesWithMessageType<Message1>(EventType.MessageSucceeded).Single();
            record
                .ShouldNotBeNull();

            record.Envelope.Source.ShouldBe(theSender.Get<JasperOptions>().ServiceName);
        }

        [Fact]
        public async Task tracking_correlation_id_on_everything()
        {

            var id2 = string.Empty;
            var session2 = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)

                .ExecuteAndWaitAsync(async context =>
                {
                    id2 = context.CorrelationId;

                    await context.SendAsync(new ExecutedMessage());
                    await context.PublishAsync(new ExecutedMessage());
                    //await context.ScheduleSend(new ExecutedMessage(), DateTime.UtcNow.AddDays(5));
                });

            var envelopes = session2
                .AllRecordsInOrder(EventType.Sent)
                .Select(x => x.Envelope)
                .ToArray();


            foreach (var envelope in envelopes) envelope.CorrelationId.ShouldBe(id2);
        }

        [Fact]
        public async Task schedule_send()
        {
            var session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .Timeout(15.Seconds())
                .WaitForMessageToBeReceivedAt<ColorChosen>(theReceiver ?? theSender)
                .ExecuteAndWaitAsync(c => c.ScheduleSendAsync(new ColorChosen {Name = "Orange"}, 5.Seconds()));

            var message = session.FindSingleTrackedMessageOfType<ColorChosen>(EventType.MessageSucceeded);
            message.Name.ShouldBe("Orange");
        }


        protected void throwOnAttempt<T>(int attempt) where T : Exception, new()
        {
            theMessage.Errors.Add(attempt, new T());
        }

        protected async Task<EnvelopeRecord> afterProcessingIsComplete()
        {
            _session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .SendMessageAndWaitAsync(theMessage);

            return _session.AllRecordsInOrder().LastOrDefault(x =>
                x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);

        }

        protected async Task shouldSucceedOnAttempt(int attempt)
        {
            var session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .Timeout(15.Seconds())
                .DoNotAssertOnExceptionsDetected()
                .SendMessageAndWaitAsync(theMessage);

            var record = session.AllRecordsInOrder().LastOrDefault(x =>
                x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);

            if (record == null) throw new Exception("No ending activity detected");

            if (record.EventType == EventType.MessageSucceeded && record.AttemptNumber == attempt)
            {
                return;
            }

            var writer = new StringWriter();

            await writer.WriteLineAsync($"Actual ending was '{record.EventType}' on attempt {record.AttemptNumber}");
            foreach (var envelopeRecord in session.AllRecordsInOrder())
            {
                writer.WriteLine(envelopeRecord);
                if (envelopeRecord.Exception != null)
                {
                    await writer.WriteLineAsync(envelopeRecord.Exception.Message);
                }
            }

            throw new Exception(writer.ToString());
        }

        protected async Task shouldMoveToErrorQueueOnAttempt(int attempt)
        {
            var session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .Timeout(30.Seconds())
                .SendMessageAndWaitAsync(theMessage);

            var record = session.AllRecordsInOrder().LastOrDefault(x =>
                x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);

            if (record == null) throw new Exception("No ending activity detected");

            if (record.EventType == EventType.MovedToErrorQueue && record.AttemptNumber == attempt)
            {
                return;
            }

            var writer = new StringWriter();

            writer.WriteLine($"Actual ending was '{record.EventType}' on attempt {record.AttemptNumber}");
            foreach (var envelopeRecord in session.AllRecordsInOrder())
            {
                writer.WriteLine(envelopeRecord);
                if (envelopeRecord.Exception != null)
                {
                    writer.WriteLine(envelopeRecord.Exception.Message);
                }
            }

            throw new Exception(writer.ToString());
        }


        [Fact]
        public virtual async Task will_move_to_dead_letter_queue_without_any_exception_match()
        {
            throwOnAttempt<InvalidOperationException>(1);
            throwOnAttempt<InvalidOperationException>(2);
            throwOnAttempt<InvalidOperationException>(3);

            await shouldMoveToErrorQueueOnAttempt(3);
        }

        [Fact]
        public virtual async Task will_move_to_dead_letter_queue_with_exception_match()
        {
            throwOnAttempt<DivideByZeroException>(1);
            throwOnAttempt<DivideByZeroException>(2);
            throwOnAttempt<DivideByZeroException>(3);

            await shouldMoveToErrorQueueOnAttempt(1);
        }


        [Fact]
        public virtual async Task will_requeue_and_increment_attempts()
        {
            throwOnAttempt<DataMisalignedException>(1);
            throwOnAttempt<DataMisalignedException>(2);

            await shouldSucceedOnAttempt(3);
        }

        [Fact]
        public async Task can_retry_later()
        {
            throwOnAttempt<BadImageFormatException>(1);

            await shouldSucceedOnAttempt(2);
        }


        [Fact]
        public async Task explicit_respond_to_sender()
        {
            var ping = new PingMessage();

            var session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .Timeout(30.Seconds())
                .SendMessageAndWaitAsync(ping);

            session.FindSingleTrackedMessageOfType<PongMessage>(EventType.MessageSucceeded)
                .Id.ShouldBe(ping.Id);
        }

        [Fact]
        public async Task requested_response()
        {
            var ping = new ImplicitPing();

            var session = await theSender
                .TrackActivity(Fixture.DefaultTimeout)
                .AlsoTrack(theReceiver)
                .Timeout(30.Seconds())
                .ExecuteAndWaitAsync(x => x.SendAndExpectResponseForAsync<ImplicitPong>(ping));

            session.FindSingleTrackedMessageOfType<ImplicitPong>(EventType.MessageSucceeded)
                .Id.ShouldBe(ping.Id);
        }

        [Fact] // This test isn't always the most consistent test
        public async Task send_green_as_text_and_receive_as_blue()
        {
            if (Fixture.AllLocally) return; // this just doesn't apply when running all with local queues

            var greenMessage = new GreenMessage {Name = "Magic Johnson"};
            var envelope = new Envelope(greenMessage)
            {
                ContentType = "text/plain"
            };

            var session = await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .ExecuteAndWaitAsync(c => c.SendEnvelopeAsync(envelope));

            session.FindSingleTrackedMessageOfType<BlueMessage>()
                .Name.ShouldBe("Magic Johnson");
        }

        [Fact]
        public async Task send_green_that_gets_received_as_blue()
        {
            if (Fixture.AllLocally) return; // this just doesn't apply when running all with local queues

            var session = await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .ExecuteAndWaitAsync(c =>
                    c.SendAsync(new GreenMessage {Name = "Kareem Abdul-Jabbar"}));


            session.FindSingleTrackedMessageOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul-Jabbar");
        }

    }


    #region sample_BlueTextReader
    public class BlueTextReader : IMessageSerializer
    {
        public BlueTextReader()
        {
        }

        public string ContentType { get; } = "text/plain";
        public byte[] Write(object message)
        {
            throw new NotImplementedException();
        }

        public object? ReadFromData(Type messageType, byte[]? data)
        {
            return ReadFromData(data);
        }

        public object? ReadFromData(byte[]? data)
        {
            var name = Encoding.UTF8.GetString(data);
            return new BlueMessage {Name = name};
        }
    }
    #endregion

        #region sample_GreenTextWriter
        public class GreenTextWriter : IMessageSerializer
        {
            public string? ContentType { get; } = "text/plain";

            public object? ReadFromData(Type messageType, byte[]? data)
            {
                throw new NotImplementedException();
            }

            public object? ReadFromData(byte[]? data)
            {
                throw new NotImplementedException();
            }

            public byte[] Write(object model)
            {
                if (model is GreenMessage green) return Encoding.UTF8.GetBytes(green.Name);

                throw new NotSupportedException("This serializer only writes GreenMessage");
            }
        }
        #endregion
}
