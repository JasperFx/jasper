using System;
using Jasper;
using Jasper.CommandLine;
using Jasper.Messaging.Runtime.Invocation;
using Oakton;
using TestMessages;

namespace Ponger
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperAgent.Run(args, _ => { _.Transports.LightweightListenerAt(2601); });
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
