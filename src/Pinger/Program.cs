using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using TestMessages;

namespace Pinger
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            return JasperHost.Run(args, _ =>
            {
                _.Transports.LightweightListenerAt(2600);

                // Using static routing rules to start
                _.Publish.Message<PingMessage>().To("tcp://localhost:2601");

                _.Services.AddSingleton<IHostedService, PingSender>();

                _.Hosting(x =>
                    x.UseUrls("http://localhost:5000")
                    .UseKestrel());
            });
        }
    }

    // SAMPLE: AppThatUsesPingHandler
    public class AppThatUsesPingHandler : JasperRegistry
    {
        public AppThatUsesPingHandler()
        {
            // Just register your custom hosted service
            // as a singleton in the IoC container
            // against the IHostedService interface
            Services.AddSingleton<IHostedService, PingSender>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: PongHandler
    public class PongHandler
    {
        public void Handle(PongMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a pong back with name: " + message.Name);
        }
    }
    // ENDSAMPLE

    public class HomeEndpoint
    {
        public string Index()
        {
            return "Hello!";
        }

        public string get_hello()
        {
            return "hello.";
        }
    }

    // SAMPLE: PingSender
    // In this case, BackgroundService is a base class
    // for the IHostedService that is *supposed* to be
    // in a future version of ASP.Net Core that I shoved
    // into Jasper so we could use it now. The one in Jasper
    // will be removed later when the real one exists in
    // ASP.Net Core itself
    public class PingSender : BackgroundService
    {
        private readonly IMessageContext _bus;

        public PingSender(IMessageContext bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var count = 1;

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);

                    await _bus.Send(new PingMessage
                    {
                        Name = "Message" + count++
                    });
                }
            }, stoppingToken);
        }
    }

    // ENDSAMPLE
}
