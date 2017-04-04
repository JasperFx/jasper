using System;
using DiagnosticsHarnessMessages;
using Jasper;
using Jasper.Diagnostics;
using JasperBus;
using PeterKottas.DotNetCore.WindowsService.Interfaces;

namespace DiagnosticsServiceHarness
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("hello world");

            using (var service = new MyMicroService())
            {
                service.Start();
                Console.ReadKey();
            }
        }
    }

    public class MyMicroService : IMicroService, IDisposable
    {
        private JasperRuntime _runtime;

        public void Start()
        {
            _runtime = JasperRuntime.For<BusRegistry>(_ =>
            {
            });
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }

    public class BusRegistry : JasperBusRegistry
    {
        public BusRegistry()
        {
            var uri = "lq.tcp://localhost:2110/servicebus_example";
            ListenForMessagesFrom(uri);

            Logging.UseConsoleLogging = true;

            Feature<DiagnosticsFeature>();

            //  this.AddDiagnostics();
        }
    }

    public class MiddlewareMessageConsumer
    {
        public void Consume(MiddlewareMessage message)
        {
            Console.WriteLine($"Got Message: {message.Message}");
        }
    }

    public class SomeConsumer
    {
        public void Consume(AMessageThatWillError message)
        {
            throw new NotSupportedException();
        }
    }
}
