using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Baseline;
using InteropMessages;
using Jasper;
using Jasper.Configuration;
using Jasper.RabbitMQ;
using Jasper.Tracking;
using MassTransit;
using MassTransit.RabbitMqTransport.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace InteroperabilityTests
{
    public class MassTransitFixture : IAsyncLifetime
    {
        private IHost _massTransit;

        public async Task InitializeAsync()
        {
            Jasper = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                // application/vnd.masstransit+json

                opts.ConfigureRabbitMq(t =>
                {
                    t.AutoProvision = true;
                    t.AutoPurgeOnStartup = true;
                    t.DeclareQueue("jasper"); // TODO -- make this inferred
                    t.DeclareQueue("masstransit"); // TODO -- make this inferred

                    t.DeclareExchange("jasper", x => x.ExchangeType = ExchangeType.Fanout);
                    t.DeclareBinding(new Binding
                    {
                        QueueName = "jasper",
                        ExchangeName = "jasper",
                        BindingKey = "jasper"
                    });

                    t.DeclareExchange("masstransit", x => x.ExchangeType = ExchangeType.Fanout);
                    t.DeclareBinding(new Binding
                    {
                        QueueName = "masstransit",
                        ExchangeName = "masstransit",
                        BindingKey = "masstransit"
                    });
                });

                opts.PublishAllMessages().ToRabbit("masstransit")
                    .Advanced(endpoint =>
                    {
                        // TODO -- will need access to the RabbitMqTransport to get the reply endpoint, then
                        // write out the MT version of the Uri
                        endpoint.MapOutgoingProperty(x => x.ReplyUri, (e, p) =>
                        {
                            // TODO -- this will need to be cached somehow
                            p.Headers[MassTransitHeaders.ResponseAddress] = "rabbitmq://localhost/jasper";

                        });
                    });

                opts.ListenToRabbitQueue("jasper")
                    .DefaultIncomingMessage<ResponseMessage>();

                opts.Extensions.UseMessageTrackingTestingSupport();
            }).StartAsync();

            _massTransit = await MassTransitService.Program.CreateHostBuilder(Array.Empty<string>())
                .StartAsync();
        }

        public IHost MassTransit => _massTransit;

        public IHost Jasper { get; private set; }

        public async Task DisposeAsync()
        {
            await Jasper.StopAsync();
            await _massTransit.StopAsync();

        }
    }

    public class MassTransitSpecs : IClassFixture<MassTransitFixture>
    {
        private readonly MassTransitFixture theFixture;

        public MassTransitSpecs(MassTransitFixture fixture)
        {
            theFixture = fixture;
        }

        [Fact]
        public async Task masstransit_sends_message_to_jasper()
        {
            ResponseHandler.Received.Clear();

            var id = Guid.NewGuid();

            // TODO -- set up a missing handler

            var session = await theFixture.Jasper.ExecuteAndWaitAsync(async () =>
            {
                var sender = theFixture.MassTransit.Services.GetRequiredService<ISendEndpointProvider>();
                var endpoint = await sender.GetSendEndpoint(new Uri("rabbitmq://localhost/jasper"));
                await endpoint.Send(new ResponseMessage {Id = id});
            });

            var envelope = ResponseHandler.Received.FirstOrDefault();
            envelope.Message.ShouldBeOfType<ResponseMessage>().Id.ShouldBe(id);
            envelope.ShouldNotBeNull();
        }

        [Fact]
        public async Task jasper_sends_message_to_masstransit_that_then_responds()
        {
            ResponseHandler.Received.Clear();

            var id = Guid.NewGuid();

            var session = await theFixture.Jasper.TrackActivity()
                .WaitForMessageToBeReceivedAt<ResponseMessage>(theFixture.Jasper)
                .SendMessageAndWait(new InitialMessage {Id = id});

            ResponseHandler.Received
                .Select(x => x.Message)
                .OfType<ResponseMessage>()
                .Any(x => x.Id == id)
                .ShouldBeTrue();
      }


    }


}
