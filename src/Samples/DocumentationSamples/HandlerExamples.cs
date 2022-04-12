using Jasper;
using Jasper.Attributes;
using Marten;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using TestMessages;

namespace DocumentationSamples
{
    #region sample_ValidMessageHandlers
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

        public interface IEvent
        {
            string CustomerId { get; }
            Guid Id { get; }
        }
    }
    #endregion


    public interface IEmailService
    {
    }


    #region sample_simplest_possible_handler
    public class MyMessageHandler
    {
        public void Handle(MyMessage message)
        {
            // do stuff with the message
        }
    }
    #endregion


    namespace One
    {
        #region sample_ExampleHandlerByInstance
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

        #endregion
    }

    namespace Two
    {
        #region sample_ExampleHandlerByStaticMethods
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

        #endregion
    }

    namespace Sample2
    {
         [JasperIgnore]
        #region sample_HandlerBuiltByConstructorInjection
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
        #endregion
    }

    namespace Three
    {
        [JasperIgnore]
        #region sample_HandlerUsingMethodInjection
        public static class MethodInjectionHandler
        {
            public static Task Handle(InvoiceCreated message, IDocumentSession session)
            {
                var invoice = new Invoice {Id = message.InvoiceId};
                session.Store(invoice);

                return session.SaveChangesAsync();
            }
        }

        #endregion
    }

    #region sample_HandlerUsingEnvelope
    public class EnvelopeUsingHandler
    {
        public void Handle(InvoiceCreated message, Envelope envelope)
        {
            var howOldIsThisMessage =
                DateTime.UtcNow.Subtract(envelope.SentAt);
        }
    }
    #endregion


    public class Invoice
    {
        public Guid Id { get; set; }
    }

    public static class HandlerExamples
    {
        public static async Task explicit_handler_discovery()
        {
            #region sample_ExplicitHandlerDiscovery

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // No automatic discovery of handlers
                    opts.Handlers.DisableConventionalDiscovery();
                }).StartAsync();

            #endregion
        }
    }

}
