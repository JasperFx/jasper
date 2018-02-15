using System;
using Jasper.CommandLine;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports.Configuration;
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
