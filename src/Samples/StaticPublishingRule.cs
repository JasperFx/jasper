using System.Threading.Tasks;
using Jasper;
using Jasper.Tcp;
using Microsoft.Extensions.Hosting;
using TestingSupport.Compliance;
using TestMessages;

namespace Samples
{
    public static class static_publishing_rules
    {
        public static async Task StaticPublishingRules()
        {
            #region sample_StaticPublishingRules

            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Publish(rule =>
                    {
                        // Apply as many message matching
                        // rules as you need

                        // Specific message types
                        rule.Message<PingMessage>();
                        rule.Message<Message1>();

                        // All types in a certain assembly
                        rule.MessagesFromAssemblyContaining<PingMessage>();

                        // or this
                        rule.MessagesFromAssembly(typeof(PingMessage).Assembly);

                        // or by namespace
                        rule.MessagesFromNamespace("MyMessageLibrary");
                        rule.MessagesFromNamespaceContaining<PingMessage>();

                        // Express the subscribers
                        rule.ToPort(1111);
                        rule.ToPort(2222);
                    });

                    // Or you just send all messages to a certain endpoint
                    opts.PublishAllMessages().ToPort(3333);
                }).StartAsync();

            #endregion
        }
    }
}
