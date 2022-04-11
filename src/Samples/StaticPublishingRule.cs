using Jasper;
using Jasper.Tcp;
using TestingSupport.Compliance;
using TestMessages;

namespace Samples
{
    // SAMPLE: StaticPublishingRules
    public class StaticPublishingRulesApp : JasperOptions
    {
        public StaticPublishingRulesApp()
        {
            Publish(x =>
            {
                // Apply as many message matching
                // rules as you need

                // Specific message types
                x.Message<PingMessage>();
                x.Message<Message1>();

                // All types in a certain assembly
                x.MessagesFromAssemblyContaining<PingMessage>();

                // or this
                x.MessagesFromAssembly(typeof(PingMessage).Assembly);

                // or by namespace
                x.MessagesFromNamespace("MyMessageLibrary");
                x.MessagesFromNamespaceContaining<PingMessage>();

                // Express the subscribers
                x.ToPort(1111);
                x.ToPort(2222);
            });

            // Or you just send all messages to a certain endpoint
            PublishAllMessages().ToPort(3333);
        }
        // ENDSAMPLE
    }
}
