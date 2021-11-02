using System.Diagnostics;

namespace Jasper
{
    internal static class JasperTracing
    {
        public const string MessageExecution = "message-execution";

        internal static ActivitySource ActivitySource = new ActivitySource(
            "Jasper",
            typeof(JasperTracing).Assembly.GetName().Version.ToString());

        public static Activity StartExecution(Envelope envelope)
        {
            // span name should be: <destination name> <operation name>
            // Bring back Envelope.ReceivedAt!!! But don't persist.
            // Nah, use service name^messagetype
            var activity = ActivitySource.StartActivity(MessageExecution);



            /*
             * See https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md
             *
             * span kind -> producer if you send it async, client if it's request/response. Consumer if you're receiving it
             * Operation names: send, receive, process
             *
             *
             *
             */

            // What tags do we want to add here?

            return activity;
        }
    }
}
