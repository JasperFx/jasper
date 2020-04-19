using System.Reflection;
using System.Threading.Tasks;
using Baseline.Reflection;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.ContentHandling
{
    public class write_status_code_returned_from_an_action : RegistryContext<HttpTestingApp>
    {
        public write_status_code_returned_from_an_action(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }


        [Fact]
        public void async_int_returning_action_is_action_candidate()
        {
            var method = ReflectionHelper.GetMethod<StatusCodeEndpoint>(x => x.get_status2());
            new JasperHttpOptions().MethodFilters.Matches(method).ShouldBeTrue();
        }

        [Fact]
        public Task set_status_from_async_action()
        {
            return scenario(_ =>
            {
                _.Get.Url("/status2");
                _.StatusCodeShouldBe(203);
            });
        }

        // SAMPLE: StatusCodeEndpointSpec
        [Fact]
        public Task set_status_from_sync_action()
        {
            return scenario(_ =>
            {
                _.Get.Url("/status1");
                _.StatusCodeShouldBe(201);
            });
        }
        // ENDSAMPLE

        [Fact]
        public void sync_int_returning_action_is_action_candidate()
        {
            var method = typeof(StatusCodeEndpoint).GetMethod(nameof(StatusCodeEndpoint.get_status1),
                BindingFlags.Public | BindingFlags.Static);

            new JasperHttpOptions().MethodFilters.Matches(method).ShouldBeTrue();
        }
    }

    // SAMPLE: StatusCodeEndpoint
    public class StatusCodeEndpoint
    {
        public static int get_status1()
        {
            return 201;
        }

        public Task<int> get_status2()
        {
            return Task.FromResult(203);
        }
    }

    // ENDSAMPLE
}
