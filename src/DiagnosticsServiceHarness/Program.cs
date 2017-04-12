using System;
using DiagnosticsHarnessMessages;
using Jasper;
using Jasper.Diagnostics;
using JasperBus;

namespace DiagnosticsServiceHarness
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var service = new MyMicroService())
            {
                service.Start();
                Console.Read();
            }
        }
    }

    public class MyMicroService : IDisposable
    {
        private JasperRuntime _runtime;

        public void Start()
        {
            _runtime = JasperRuntime.For<BusRegistry>();
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

            Settings.Alter<DiagnosticsSettings>(_ =>
            {
                _.WebsocketPort = 3300;
            });

            Feature<DiagnosticsFeature>();
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
