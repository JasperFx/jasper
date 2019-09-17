using System;
using System.Threading.Tasks;
using Jasper;
using Jasper.CommandLine;
using Jasper.Messaging.Runtime.Invocation;
using Oakton;
using TestMessages;

namespace Ponger
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            return JasperHost.Run(args, _ => { _.Transports.LightweightListenerAt(2601); });
        }
    }


    // SAMPLE: PingHandler
    public class PingHandler
    {
        public Response Handle(PingMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a ping with name: " + message.Name);

            var response = new PongMessage
            {
                Name = message.Name
            };

            // Don't know if you'd use this very often,
            // but this is a special syntax that will send
            // the "response" back to the original sender
            return Respond.With(response).ToSender();
        }
    }
    // ENDSAMPLE
}
