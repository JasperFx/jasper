using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using OpenTelemetry.Trace;
using OtelMessages;

namespace Subscriber1
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseJasper(opts =>
                {
                    opts.ServiceName = "subscriber1";
                    opts.UseRabbitMq();

                    opts.ListenToRabbitQueue("subscriber1");

                    opts.Services.AddOpenTelemetryTracing(builder =>
                    {
                        builder
                            .AddJaegerExporter()
                            .AddJasper();
                    });

                }).RunOaktonCommands(args);
        }

        public class InlineMessageHandler
        {
            private static Random _random = new Random();

            public void Handle(InlineMessage message)
            {
                Thread.Sleep(_random.Next(10, 200));
            }
        }


    }
}
