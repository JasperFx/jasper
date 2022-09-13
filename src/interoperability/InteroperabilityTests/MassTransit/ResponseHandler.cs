using System.Collections.Generic;
using InteropMessages;
using Jasper;

namespace InteroperabilityTests.MassTransit
{
    public class ResponseHandler
    {
        public static IList<Envelope> Received = new List<Envelope>();

        public static void Handle(ResponseMessage message, Envelope envelope)
        {
            Received.Add(envelope);
        }
    }
}
