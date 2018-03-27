using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Alba;
using Jasper.Http.Routing;
using Jasper.Testing;
using JasperHttpTesting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Routing
{
    public class route_determination_and_usage_with_arguments : IDisposable
    {
        private JasperRuntime _runtime;

        public route_determination_and_usage_with_arguments()
        {

        }

        private async Task withApp()
        {
            _runtime = await JasperRuntime.ForAsync<JasperRegistry>(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.HttpRoutes.IncludeType<RouteEndpoints>();
            });
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }

        private Route routeFor(Expression<Action<RouteEndpoints>> expression)
        {
            return RouteBuilder.Build(expression);
        }


        [Fact]
        public async Task int_argument()
        {
            await withApp();

            var route = routeFor(x => x.get_int_number(5));
            route.HttpMethod.ShouldBe("GET");
            route.Pattern.ShouldBe("int/:number");

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/int/5");
                _.ContentShouldBe("5");
            });
        }

        [Fact]
        public async Task long_argument()
        {
            await withApp();

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/long/11");
                _.ContentShouldBe("11");
            });
        }

        [Fact]
        public async Task double_argument()
        {
            await withApp();

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/double/1.23");
                _.ContentShouldBe("1.23");
            });
        }

        [Fact]
        public async Task char_arguments()
        {
            await withApp();

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/letters/f/to/k");
                _.ContentShouldBe("f-k");
            });
        }

        [Fact]
        public async Task bool_arguments()
        {
            await withApp();

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/bool/true");
                _.ContentShouldBe("blue");
            });

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/bool/false");
                _.ContentShouldBe("green");
            });
        }

        [Fact]
        public async Task guid_argument()
        {
            await withApp();

            var id = Guid.NewGuid().ToString();

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/guid/" + id);
                _.ContentShouldBe($"*{id}*");
            });
        }


    }

    public class RouteEndpoints
    {
        public string get_date_id(DateTime id)
        {
            return id.ToString("r");
        }

        public string get_guid_id(Guid id)
        {
            return $"*{id}*";
        }

        public string get_bool_value(bool value)
        {
            return value ? "blue" : "green";
        }

        public string get_double_number(double number)
        {
            return number.ToString();
        }

        public string get_int_number(int number)
        {
            return number.ToString();
        }

        public string get_long_number(long number)
        {
            return number.ToString();
        }

        public string get_letters_first_to_end(char first, char end, HttpRequest request)
        {
            return $"{first}-{end}";
        }
    }
}
