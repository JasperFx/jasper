using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Samples;

namespace Jasper.Testing.Samples.HandlerDiscovery
{
    // SAMPLE: SimpleHandler
    public class SimpleHandler
    {
        public void Handle(PingMessage message)
        {
            Console.WriteLine("I got a ping!");
        }
    }
    // ENDSAMPLE


    // SAMPLE: AsyncHandler
    public interface IPongWriter
    {
        Task WritePong(PongMessage message);
    }

    public class AsyncHandler
    {
        private readonly IPongWriter _writer;

        public AsyncHandler(IPongWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(PongMessage message)
        {
            return _writer.WritePong(message);
        }
    }
    // ENDSAMPLE

    // SAMPLE: Handlers-IMessage
    public interface IMessage
    {
    }

    public class MessageOne : IMessage
    {
    }
    // ENDSAMPLE

    // SAMPLE: Handlers-GenericMessageHandler
    public class GenericMessageHandler
    {
        private readonly Envelope _envelope;

        public GenericMessageHandler(Envelope envelope)
        {
            _envelope = envelope;
        }

        public void Consume(IMessage message)
        {
            Console.WriteLine($"Got a message from {_envelope.Source}");
        }
    }
    // ENDSAMPLE

    // SAMPLE: Handlers-SpecificMessageHandler
    public class SpecificMessageHandler
    {
        public void Consume(MessageOne message)
        {
        }
    }
    // ENDSAMPLE

    // SAMPLE: injecting-services-into-handlers
    public interface IMyService
    {
    }

    public class ServiceUsingHandler
    {
        private readonly IMyService _service;

        // Using constructor injection to get dependencies
        public ServiceUsingHandler(IMyService service)
        {
            _service = service;
        }

        public void Consume(PingMessage message)
        {
            // do stuff using IMyService with the PingMessage
            // input
        }
    }
    // ENDSAMPLE

    // SAMPLE: IHandler_of_T
    public interface IHandler<T>
    {
        void Handle(T message);
    }
    // ENDSAMPLE


    // SAMPLE: CustomHandlerApp
    public class CustomHandlerApp : JasperRegistry
    {
        public CustomHandlerApp()
        {
            // Turn off the default handler conventions
            // altogether
            Handlers.DisableConventionalDiscovery();


            // Include candidate actions by a user supplied
            // type filter
            Handlers.IncludeTypes(t => t.IsInNamespace("MyApp.Handlers"));

            // Include candidate classes by suffix
            Handlers.IncludeClassesSuffixedWith("Listener");

            // Include a specific handler class with a generic argument
            Handlers.IncludeType<SimpleHandler>();
        }
    }

    // ENDSAMPLE
}
