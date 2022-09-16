using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using InteropMessages;
using Jasper;
using Jasper.RabbitMQ;
using Jasper.Runtime.Interop.MassTransit;
using Jasper.Tracking;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace InteroperabilityTests.MassTransit
{
    public class MassTransitFixture : IAsyncLifetime
    {
        private IHost _massTransit;

        public async Task InitializeAsync()
        {
            Jasper = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                // application/vnd.masstransit+json

                opts.UseRabbitMq()
                    .AutoProvision().AutoPurgeOnStartup()
                    .BindExchange("jasper").ToQueue("jasper")
                    .BindExchange("masstransit").ToQueue("masstransit");

                opts.PublishAllMessages().ToRabbitExchange("masstransit")
                    .Advanced(endpoint =>
                    {
                        endpoint.UseMassTransitInterop();
                    });

                opts.ListenToRabbitQueue("jasper").UseMassTransitInterop()
                    .DefaultIncomingMessage<ResponseMessage>().UseForReplies();

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
            }, 60000);

            var envelope = ResponseHandler.Received.FirstOrDefault();
            envelope.Message.ShouldBeOfType<ResponseMessage>().Id.ShouldBe(id);
            envelope.ShouldNotBeNull();
        }

        [Fact]
        public async Task jasper_sends_message_to_masstransit_that_then_responds()
        {
            ResponseHandler.Received.Clear();

            var id = Guid.NewGuid();

            var session = await theFixture.Jasper.TrackActivity().Timeout(10.Minutes())
                .WaitForMessageToBeReceivedAt<ResponseMessage>(theFixture.Jasper)
                .SendMessageAndWaitAsync(new InitialMessage {Id = id});

            ResponseHandler.Received
                .Select(x => x.Message)
                .OfType<ResponseMessage>()
                .Any(x => x.Id == id)
                .ShouldBeTrue();
      }


    }


}
