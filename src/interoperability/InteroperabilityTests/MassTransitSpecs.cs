using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Baseline;
using InteropMessages;
using Jasper;
using Jasper.Tracking;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            Jasper = await Host.CreateDefaultBuilder().UseJasper<JasperApp>().StartAsync();

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

            var session = await theFixture.Jasper.ExecuteAndWait(async () =>
            {
                var sender = theFixture.MassTransit.Services.GetRequiredService<ISendEndpointProvider>();
                var endpoint = await sender.GetSendEndpoint(new Uri("rabbitmq://localhost/jasper"));
                await endpoint.Send(new ResponseMessage {Id = id});
            });

            var envelope = ResponseHandler.Received.FirstOrDefault();
            envelope.ShouldNotBeNull();
        }
    }


}
