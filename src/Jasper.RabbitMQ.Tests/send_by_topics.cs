using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Attributes;
using Jasper.Tracking;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class send_by_topics : IDisposable
    {
        private readonly IHost theSender;
        private readonly IHost theFirstReceiver;
        private readonly IHost theSecondReceiver;
        private readonly IHost theThirdReceiver;

        public send_by_topics()
        {
            theSender = JasperHost.For(opts =>
            {
                opts.UseRabbitMq().AutoProvision();
                opts.PublishAllMessages().ToRabbitTopics("TopicRouter", exchange =>
                {
                    exchange.BindTopic("color.green").ToQueue("green");
                    exchange.BindTopic("color.blue").ToQueue("blue");
                    exchange.BindTopic("color.all").ToQueue("all");
                });
            });

            theFirstReceiver = JasperHost.For(opts =>
            {
                opts.ServiceName = "First";
                opts.ListenToRabbitQueue("green" );
                opts.UseRabbitMq();
            });

            theSecondReceiver = JasperHost.For(opts =>
            {
                opts.ServiceName = "Second";
                opts.ListenToRabbitQueue( "blue");
                opts.UseRabbitMq();
            });

            theThirdReceiver = JasperHost.For(opts =>
            {
                opts.ServiceName = "Third";
                opts.ListenToRabbitQueue("all");
                opts.UseRabbitMq();
            });
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theFirstReceiver?.Dispose();
            theSecondReceiver?.Dispose();
            theThirdReceiver?.Dispose();
        }

        [Fact]
        public async Task send_by_message_topic()
        {
            var session = await theSender
                .TrackActivity()
                .IncludeExternalTransports()
                .AlsoTrack(theFirstReceiver, theSecondReceiver, theThirdReceiver)
                .SendMessageAndWaitAsync(new PurpleMessage());

            session.FindEnvelopesWithMessageType<PurpleMessage>()
                .Where(x => x.EventType == EventType.Received)
                .Select(x => x.ServiceName)
                .Single().ShouldBe("Third");

        }

        [Fact]
        public async Task send_by_explicit_topic()
        {
            var session = await theSender
                .TrackActivity()
                .IncludeExternalTransports()
                .AlsoTrack(theFirstReceiver, theSecondReceiver, theThirdReceiver)
                .SendMessageToTopicAndWaitAsync("color.green", new PurpleMessage());

            session.FindEnvelopesWithMessageType<PurpleMessage>()
                .Where(x => x.EventType == EventType.Received)
                .Select(x => x.ServiceName)
                .OrderBy(x => x)
                .ShouldHaveTheSameElementsAs("First", "Third");

        }
    }

    [Topic("color.purple")]
    public class PurpleMessage{}

    public class FirstMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class SecondMessage : FirstMessage
    {

    }

    public class ThirdMessage : FirstMessage
    {

    }

    public class MessagesHandler
    {
        public void Handle(FirstMessage message)
        {

        }

        public void Handle(SecondMessage message)
        {

        }

        public void Handle(ThirdMessage message)
        {

        }
    }
}
