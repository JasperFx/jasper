using System;
using System.Threading.Tasks;
using Jasper;
using Oakton;

namespace Ponger
{
    #region sample_PingHandler

    public static class PingHandler
    {
        // Simple message handler for the PingMessage message type
        public static ValueTask Handle(
            // The first argument is assumed to be the message type
            PingMessage message,

            // Jasper supports method injection similar to ASP.Net Core MVC
            // In this case though, IMessageContext is scoped to the message
            // being handled
            IMessageContext context)
        {
            ConsoleWriter.Write(ConsoleColor.Blue, $"Got ping #{message.Number}");

            var response = new PongMessage
            {
                Number = message.Number
            };

            // This usage will send the response message
            // back to the original sender. Jasper uses message
            // headers to embed the reply address for exactly
            // this use case
            return context.RespondToSenderAsync(response);
        }
    }

    #endregion
}
