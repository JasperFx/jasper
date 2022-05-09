﻿using System.Threading.Tasks;
using Jasper.Attributes;
using Jasper.Tracking;
using Jasper.Util;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Tcp.Tests
{
    public class message_forwarding
    {
        [Fact]
        public async Task send_message_via_forwarding()
        {


            var host = JasperHost.For(opts =>
            {
                opts.Handlers.DisableConventionalDiscovery();
                opts.Handlers.IncludeType<NewMessageHandler>();

                opts.Publish(x =>
                {
                    x.Message<OriginalMessage>();
                    x.ToPort(2345);
                });

                opts.ListenAtPort(2345);
            });

            try
            {
                var originalMessage = new OriginalMessage {FirstName = "James", LastName = "Worthy"};
                var envelope = new Envelope(originalMessage)
                {
                    Destination = "tcp://localhost:2345".ToUri(),
                    ContentType = EnvelopeConstants.JsonContentType
                };

                var session = await host
                    .TrackActivity()
                    .IncludeExternalTransports()
                    .ExecuteAndWaitAsync(c => c.SendEnvelopeAsync(envelope));


                session.FindSingleTrackedMessageOfType<NewMessage>(EventType.MessageSucceeded)
                    .FullName.ShouldBe("James Worthy");
            }
            finally
            {
                host.Dispose();
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
        public void Handle(NewMessage message)
        {
        }
    }
}
