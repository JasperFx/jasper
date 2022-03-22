// SAMPLE: QuickStartConsoleMain

using System.Threading.Tasks;
using Jasper;
using Jasper.RabbitMQ;
using Jasper.Tcp;
using LamarCodeGeneration;

namespace MyApp
{
    internal class Program
    {
        // You may need to enable C# 7.1 or higher for your project
        private static Task<int> Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppOptions
            // until the executable is stopped
            return JasperHost.Run(args, (context, opts) =>
            {
                opts.Endpoints.ListenAtPort(2222);

                opts.Endpoints.PublishAllMessages().ToPort(2224);

                opts.Advanced.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;

                opts.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.AutoProvision = true;
                    x.AutoPurgeOnStartup = true;
                    x.DeclareQueue("rabbit1");
                    x.DeclareQueue("rabbit2");
                    x.DeclareExchange("Main");
                    x.DeclareBinding(new Binding
                    {
                        BindingKey = "BKey",
                        QueueName = "queue1",
                        ExchangeName = "Main"

                    });

                });

                opts.Endpoints.ListenToRabbitQueue("rabbit1");
                opts.Endpoints.PublishAllMessages().ToRabbit("rabbit2");
            });

            // The code above is shorthand for the following:
            /*
            return Host
                .CreateDefaultBuilder()
                .UseJasper<MyAppOptions>()
                .RunOaktonCommands(args);
            */
        }
    }
}
// ENDSAMPLE
