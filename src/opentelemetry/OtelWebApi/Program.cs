using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.RabbitMQ;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OtelMessages;

namespace OtelWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseJasper(opts =>
                {
                    opts.ServiceName = "OtelWebApi";
                    opts.UseRabbitMq().AutoProvision()
                        .AutoPurgeOnStartup();

                    opts.Publish(x =>
                    {
                        x.Message<InlineMessage>().ToRabbitQueue("subscriber1");
                    });


                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public class Work
    {

    }

    public class WorkHandler
    {
        public void Handle(Work work)
        {
            Thread.Sleep(100);
        }
    }
}
