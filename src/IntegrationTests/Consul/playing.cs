using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Consul;
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    public class playing : IDisposable
    {
        private Process _process;

        public playing()
        {
//            _process = Process.Start(new ProcessStartInfo
//            {
//                FileName = "consul",
//                Arguments = "agent -dev",
//                UseShellExecute = false
//            });
//
//            _process.HasExited.ShouldBeFalse();
        }

        public void Dispose()
        {
            //_process.Kill();
        }

        //[Fact]
        public async Task try_to_connect()
        {
//            using (var client = new HttpClient())
//            {
//                var json = await client.GetStringAsync("http://localhost:8500/v1/catalog/nodes");
//                json.ShouldNotBeNull();
//            }

            var gateway = new ConsulGateway(new HttpClient(), new ConsulSettings());

//            var registration = new ServiceRegistration("foo", "http://localhost:5000".ToUri());
//            registration.AddReplyAddress("jasper://localhost:5000".ToUri());
//
//
//            await gateway.RegisterService(registration);
//
//            var services = await gateway.GetRegisteredServices();
//
//            services.Any(x => x.ServiceName == "foo").ShouldBeTrue();


            var running = new RunningService("unique", "eligibility");
            await gateway.Register(running);

            await gateway.Register(new RunningService("unique1", "eligibility"));
            await gateway.Register(new RunningService("unique2", "eligibility"));
            await gateway.Register(new RunningService("unique3", "eligibility"));

            await gateway.UnRegister(running.ID);

            await gateway.SetProperty("foo", "Something Else");

            var value = await gateway.GetProperty("foo");

            value.ShouldBe("Something Else");
        }
    }
}
