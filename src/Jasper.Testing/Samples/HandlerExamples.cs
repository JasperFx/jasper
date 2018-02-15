using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Testing.Samples.FirstTry;
using Marten;

namespace Jasper.Testing.Samples
{

    // SAMPLE: ValidMessageHandlers
    public class ValidMessageHandlers
    {
        // There's only one argument, so we'll assume that
        // argument is the message
        public void Handle(Message1 something)
        {

        }

        // The parameter named "message" is assumed to be the message type
        public void Consume(Message1 message, IDocumentSession session)
        {

        }

        // It's perfectly valid to have multiple handler methods
        // for a given message type. Each will be called in sequence
        public void SendEmail(Message1 input, IEmailService emails)
        {

        }

        public interface IEvent
        {
            string CustomerId { get; }
            Guid Id { get; }
        }

        // It's also legal to handle a message by an abstract
        // base class or an implemented interface.
        public void PostProcessEvent(IEvent @event)
        {

        }

        // In this case, we assume that the first type is the message type
        // because it's concrete, not "simple", and isn't suffixed with
        // "Settings"
        public void Consume(Message3 weirdName, IEmailService service)
        {

        }
    }
    // ENDSAMPLE




    public interface IEmailService{}

    public class MyMessage
    {

    }

    // SAMPLE: simplest-possible-handler
    public class MyMessageHandler
    {
        public void Handle(MyMessage message)
        {
            // do stuff with the message
        }
    }
    // ENDSAMPLE


    namespace One
    {
        // SAMPLE: ExampleHandlerByInstance
        public class ExampleHandler
        {
            public void Handle(Message1 message)
            {
                // Do work synchronously
            }

            public Task Handle(Message2 message)
            {
                // Do work asynchronously
                return Task.CompletedTask;
            }
        }
        // ENDSAMPLE
    }

    namespace Two
    {
        // SAMPLE: ExampleHandlerByStaticMethods
        public static class ExampleHandler
        {
            public static void Handle(Message1 message)
            {
                // Do work synchronously
            }

            public static Task Handle(Message2 message)
            {
                // Do work asynchronously
                return Task.CompletedTask;
            }
        }
        // ENDSAMPLE
    }

    [JasperIgnore]
    // SAMPLE: HandlerBuiltByConstructorInjection
    public class ServiceUsingHandler
    {
        private readonly IDocumentSession _session;

        public ServiceUsingHandler(IDocumentSession session)
        {
            _session = session;
        }

        public Task Handle(InvoiceCreated created)
        {
            var invoice = new Invoice {Id = created.InvoiceId};
            _session.Store(invoice);

            return _session.SaveChangesAsync();
        }
    }
    // ENDSAMPLE

    namespace Three
    {
        [JasperIgnore]
        // SAMPLE: HandlerUsingMethodInjection
        public static class MethodInjectionHandler
        {
            public static Task Handle(InvoiceCreated message, IDocumentSession session)
            {
                var invoice = new Invoice {Id = message.InvoiceId};
                session.Store(invoice);

                return session.SaveChangesAsync();
            }
        }
        // ENDSAMPLE
    }

    // SAMPLE: HandlerUsingEnvelope
    public class EnvelopeUsingHandler
    {
        public void Handle(InvoiceCreated message, Envelope envelope)
        {
            var howOldIsThisMessage =
                DateTime.UtcNow.Subtract(envelope.SentAt);
        }
    }
    // ENDSAMPLE


    public class Invoice
    {
        public Guid Id { get; set; }
    }


    // SAMPLE: ExplicitHandlerDiscovery
    public class ExplicitHandlerDiscovery : JasperRegistry
    {
        public ExplicitHandlerDiscovery()
        {
            // No automatic discovery of handlers
            Handlers.DisableConventionalDiscovery();
        }
    }
    // ENDSAMPLE
}
