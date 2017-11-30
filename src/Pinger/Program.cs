using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.CommandLine;
using Jasper.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using TestMessages;

namespace Pinger
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run(args, _ =>
            {
                _.Logging.UseConsoleLogging = true;

                _.Transports.LightweightListenerAt(2600);

                // Using static routing rules to start
                _.Publish.Message<PingMessage>().To("tcp://localhost:2601");

                _.Services.AddSingleton<IHostedService, PingSender>();

                _.Http
                    .UseUrls("http://localhost:5000")
                    .UseKestrel();
            });
        }
    }

    public class PongHandler
    {
        public void Handle(PongMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a pong back with name: " + message.Name);

        }
    }

    public class PingSender : BackgroundService
    {
        private readonly IServiceBus _bus;

        public PingSender(IServiceBus bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 1;

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(1000);

                    await _bus.Send(new PingMessage
                    {
                        Name = "Message" + count++
                    });
                }
            }, stoppingToken);
        }
    }
}
