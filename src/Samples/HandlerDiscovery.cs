using System;
using System.Threading.Tasks;
using Baseline;
using TestingSupport.Compliance;
using TestMessages;

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

    public class PongWriter : IPongWriter
    {
        public Task WritePong(PongMessage message)
        {
            return Task.CompletedTask;
        }
    }

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
        public void Consume(IMessage messagem, Envelope envelope)
        {
            Console.WriteLine($"Got a message from {envelope.Source}");
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

    public class MyService : IMyService
    {
    }

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
    public class CustomHandlerApp : JasperOptions
    {
        public CustomHandlerApp()
        {
            Handlers.Discovery(x =>
            {
                // Turn off the default handler conventions
                // altogether
                x.DisableConventionalDiscovery();

                // Include candidate actions by a user supplied
                // type filter
                x.IncludeTypes(t => t.IsInNamespace("MyApp.Handlers"));

                // Include candidate classes by suffix
                x.IncludeClassesSuffixedWith("Listener");

                // Include a specific handler class with a generic argument
                x.IncludeType<SimpleHandler>();
            });
        }
    }

    // ENDSAMPLE
}
