using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Alba;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Routing
{
    public class FakeThing
    {
        public string get_chars_first_to_end(char first, char end, HttpRequest request)
        {
            return $"{first}-{end}";
        }
    }


    public class route_determination_and_usage_with_arguments : RegistryContext<RoutingApp>
    {
        public route_determination_and_usage_with_arguments(RegistryFixture<RoutingApp> fixture) : base(fixture)
        {
        }

        private JasperRoute routeFor(Expression<Action<RouteEndpoints>> expression)
        {
            return JasperRoute.Build(expression);
        }

        [Fact]
        public async Task bool_arguments()
        {
            await scenario(_ =>
            {
                _.Get.Url("/bool/true");
                _.ContentShouldBe("blue");
            });

            await scenario(_ =>
            {
                _.Get.Url("/bool/false");
                _.ContentShouldBe("green");
            });
        }

        // SAMPLE: using-char-arguments
        [Fact]
        public async Task char_arguments()
        {
            await scenario(_ =>
            {
                _.Get.Url("/letters/b/to/q");
                _.ContentShouldBe("b-q");
            });
        }
        // ENDSAMPLE

        [Fact]
        public async Task double_argument()
        {
            await scenario(_ =>
            {
                _.Get.Url("/double/1.23");
                _.ContentShouldBe("1.23");
            });
        }

        [Fact]
        public async Task guid_argument()
        {
            var id = Guid.NewGuid().ToString();

            await scenario(_ =>
            {
                _.Get.Url("/guid/" + id);
                _.ContentShouldBe($"*{id}*");
            });
        }


        [Fact]
        public async Task int_argument()
        {
            var route = routeFor(x => x.get_int_number(5));
            route.HttpMethod.ShouldBe("GET");
            route.Pattern.ShouldBe("int/:number");

            await scenario(_ =>
            {
                _.Get.Url("/int/5");
                _.ContentShouldBe("5");
            });
        }

        [Fact]
        public async Task long_argument()
        {
            await scenario(_ =>
            {
                _.Get.Url("/long/11");
                _.ContentShouldBe("11");
            });
        }
    }

    public class RouteEndpoints
    {
        public string get_date_id(DateTime id)
        {
            return id.ToString("r");
        }

        // SAMPLE: using-guid-route-argument
        public string get_guid_id(Guid id)
        {
            return $"*{id}*";
        }
        // ENDSAMPLE

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

        // SAMPLE: using-multiple-arguments
        public string get_letters_first_to_end(char first, char end, HttpRequest request)
        {
            return $"{first}-{end}";
        }

        // ENDSAMPLE
    }
}
