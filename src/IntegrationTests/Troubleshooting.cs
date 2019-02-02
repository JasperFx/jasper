using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests.Persistence.Marten;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.WorkerQueues;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oakton;
using TestingSupport;

namespace IntegrationTests
{
    public class Troubleshooting
    {
        //[Fact] -- used this just once for troubleshooting RabbitMQ disconnect/reconnect issues
        public Task go()
        {
            new SqlServerEnvelopeStorageAdmin(Servers.SqlServerConnectionString, "rabbit_receiver").RecreateAll();
            new SqlServerEnvelopeStorageAdmin(Servers.SqlServerConnectionString, "rabbit_sender").RecreateAll();

            var receiver = JasperHost.For<RabbitSender>();
            var sender = JasperHost.For<RabbitReceiver>();

            var source = new TaskCompletionSource<bool>();

            return source.Task;
        }
    }

    public class TimedSender : BackgroundService
    {
        private readonly IMessageContext _context;
        private int _number;

        public TimedSender(IMessageContext context)
        {
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _context.Send(new RabbitedMessage {Number = ++_number});
                await Task.Delay(1.Seconds(), stoppingToken);
            }
        }
    }

    public class RabbitSender : JasperRegistry
    {
        public RabbitSender()
        {
            Handlers.DisableConventionalDiscovery();

            Publish.AllMessagesTo("rabbitmq://localhost:5672/numbers");

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "rabbit_sender");

            Services.AddSingleton<IHostedService, TimedSender>();
        }
    }

    public class RabbitReceiver : JasperRegistry
    {
        public RabbitReceiver()
        {
            Handlers.DisableConventionalDiscovery().IncludeType<RabbitedMessageReceiver>();

            Transports.ListenForMessagesFrom("rabbitmq://localhost:5672/numbers");

            Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "rabbit_receiver");
        }
    }

    [Durable]
    public class RabbitedMessage
    {
        public int Number { get; set; }
    }

    public class RabbitedMessageReceiver
    {
        public static void Handle(RabbitedMessage message)
        {
            Trace.WriteLine("Got " + message.Number);
            ConsoleWriter.Write(ConsoleColor.Green, "Got " + message.Number);
        }
    }
}
