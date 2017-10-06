using System;
using Jasper.CommandLine;
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

                _.Transports.Lightweight.ListenOnPort(2600);

                // Using static routing rules to start
                _.Publish.Message<PingMessage>().To("tcp://localhost:2601");
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
}
