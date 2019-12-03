using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class message_forwarding
    {
        [Fact]
        public async Task send_message_via_forwarding()
        {
            var tracker = new MessageTracker();


            var runtime = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<NewMessageHandler>();

                _.Services.AddSingleton(tracker);

                _.Endpoints.Publish(x =>
                {
                    x.Message<OriginalMessage>();
                    x.ToPort(2345);
                });

                _.Endpoints.ListenAtPort(2345);
            });

            try
            {
                var waiter = tracker.WaitFor<NewMessage>();

                await runtime.Get<IMessagePublisher>().Send(new OriginalMessage {FirstName = "James", LastName = "Worthy"}, e =>
                {
                    e.Destination = "tcp://localhost:2345".ToUri();
                    e.ContentType = "application/json";
                });

                waiter.Wait(5.Seconds());

                waiter.Result.Message.As<NewMessage>().FullName.ShouldBe("James Worthy");
            }
            finally
            {
                runtime.Dispose();
            }
        }
    }

    [MessageIdentity("versioned-message", Version = 1)]
    public class OriginalMessage : IForwardsTo<NewMessage>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public NewMessage Transform()
        {
            return new NewMessage {FullName = $"{FirstName} {LastName}"};
        }
    }

    [MessageIdentity("versioned-message", Version = 2)]
    public class NewMessage
    {
        public string FullName { get; set; }
    }

    public class NewMessageHandler
    {
        public void Handle(NewMessage message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
    }
}
