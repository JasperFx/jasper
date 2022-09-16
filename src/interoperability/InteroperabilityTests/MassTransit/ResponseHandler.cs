using System.Collections.Generic;
using System.Threading.Tasks;
using InteropMessages;
using Jasper;

namespace InteroperabilityTests.MassTransit
{
    public class ResponseHandler
    {
        public static IList<Envelope> Received = new List<Envelope>();

        public static ValueTask Handle(ResponseMessage message, Envelope envelope, IMessageContext context)
        {
            Received.Add(envelope);
            return context.RespondToSenderAsync(new ToMassTransit { Id = message.Id });
        }
    }

    public class ToJasperHandler
    {
        public static ValueTask Handle(ToJasper message, IMessageContext context)
        {
            var response = new ToMassTransit
            {
                Id = message.Id
            };

            return context.RespondToSenderAsync(response);
        }
    }
}
