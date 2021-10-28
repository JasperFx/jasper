using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Attributes;
using Jasper.Serialization;
using Jasper.Tracking;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Testing.Serialization
{
    public class NoSharedTypeFixture : IDisposable
    {
        public void Dispose()
        {
            greenApp?.Dispose();
            blueApp?.Dispose();
        }

        public IHost greenApp;
        public IHost blueApp;

        public NoSharedTypeFixture()
        {
            greenApp = JasperHost.For<GreenApp>();
            blueApp = JasperHost.For(new BlueApp());
        }
    }

    public class sending_messages_without_sharing_types : IClassFixture<NoSharedTypeFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly NoSharedTypeFixture _fixture;

        public sending_messages_without_sharing_types(ITestOutputHelper output, NoSharedTypeFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }



        [Fact] // This test isn't always the most consistent test
        public async Task send_green_as_text_and_receive_as_blue()
        {
            var greenMessage = new GreenMessage {Name = "Magic Johnson"};
            var envelope = new Envelope(greenMessage)
            {
                ContentType = "text/plain"
            };

            var session = await _fixture.greenApp
                .TrackActivity()
                .AlsoTrack(_fixture.blueApp)
                .ExecuteAndWait(c => c.SendEnvelope(envelope));


            _output.WriteLine("This is what I'm finding'");
            foreach (var record in session.AllRecordsInOrder())
            {
                _output.WriteLine(record.ToString());
            }

            session.FindSingleTrackedMessageOfType<BlueMessage>()
                .Name.ShouldBe("Magic Johnson");
        }

        [Fact]
        public async Task send_green_that_gets_received_as_blue()
        {
            var session = await _fixture.greenApp
                .TrackActivity()
                .AlsoTrack(_fixture.blueApp)
                .ExecuteAndWait(c =>
                    c.Send(new GreenMessage {Name = "Kareem Abdul Jabbar"}));


            session.FindSingleTrackedMessageOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul Jabbar");
        }
    }

    // SAMPLE: GreenTextWriter
    public class GreenTextWriter : MessageSerializerBase<GreenMessage>
    {
        public GreenTextWriter() : base("text/plain")
        {
        }

        public override byte[] Write(GreenMessage model)
        {
            return Encoding.UTF8.GetBytes(model.Name);
        }
    }
    // ENDSAMPLE

    // SAMPLE: BlueTextReader
    public class BlueTextReader : MessageDeserializerBase<BlueMessage>
    {
        public BlueTextReader() : base("text/plain")
        {
        }

        public override BlueMessage ReadData(byte[] data)
        {
            var name = Encoding.UTF8.GetString(data);
            return new BlueMessage {Name = name};
        }

        protected override async Task<BlueMessage> ReadData(Stream stream)
        {
            var name = await stream.ReadAllTextAsync();
            return new BlueMessage {Name = name};
        }
    }
    // ENDSAMPLE

    public class BlueApp : JasperOptions
    {
        public BlueApp()
        {
            Endpoints.ListenAtPort(2566);
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<BlueHandler>();
            Extensions.UseMessageTrackingTestingSupport();
        }
    }

    public class GreenApp : JasperOptions
    {
        public GreenApp()
        {
            Endpoints.Publish(x => x.Message<GreenMessage>()
                .ToPort(2566));

            Handlers.DisableConventionalDiscovery();

            Extensions.UseMessageTrackingTestingSupport();
        }
    }

    [MessageIdentity("Structural.Typed.Message")]
    public class BlueMessage
    {
        public string Name { get; set; }
    }

    [MessageIdentity("Structural.Typed.Message")]
    public class GreenMessage
    {
        public string Name { get; set; }
    }

    public class BlueHandler
    {
        public static void Consume(BlueMessage message)
        {
            Debug.WriteLine("Hey");
        }
    }
}
