using System;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.CommandLine;
using Oakton;
using TestMessages;

namespace Ponger
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run(args, _ =>
            {
                _.Logging.UseConsoleLogging = true;

                _.Transports.LightweightListenerAt(2601);
            });
        }
    }

    public class PingHandler
    {
        public object Handle(PingMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a ping with name: " + message.Name);

            var response = new PongMessage
            {
                Name = message.Name
            };

            return Respond.With(response).ToSender();
        }
    }
}
