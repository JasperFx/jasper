using System;
using System.Threading.Tasks;
using Alba;
using Baseline.Reflection;
using JasperHttp;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class route_parameter_usage : RegistryContext<HttpTestingApp>
    {
        public route_parameter_usage(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }

        [Fact]
        public void methods_are_candidate_actions()
        {
            var httpSettings = new JasperHttpOptions();

            httpSettings.MethodFilters
                .Matches(ReflectionHelper.GetMethod<RoutedEndpoint>(x => x.get_with_date_time(DateTime.MinValue)))
                .ShouldBeTrue();
            httpSettings.MethodFilters.Matches(ReflectionHelper.GetMethod<RoutedEndpoint>(x =>
                x.get_with_dateoffset_time(DateTimeOffset.MaxValue))).ShouldBeTrue();
            httpSettings.MethodFilters
                .Matches(ReflectionHelper.GetMethod<RoutedEndpoint>(x => x.get_with_number_value(55)))
                .ShouldBeTrue();
        }

        [Fact]
        public async Task use_date_time_route_arguments()
        {
            RoutedEndpoint.LastTime = DateTime.MinValue;
            var time = DateTime.Today.AddHours(3);

            await scenario(_ =>
            {
                _.Get.Url("/with/date/" + time.ToString("o"));
                _.StatusCodeShouldBeOk();
            });

            RoutedEndpoint.LastTime.ShouldBe(time);
        }

        [Fact]
        public async Task use_date_timeoffset_route_arguments()
        {
            RoutedEndpoint.LastTimeOffset = DateTimeOffset.MinValue;
            ;
            var time = new DateTimeOffset(2017, 8, 15, 8, 0, 0, 0, TimeSpan.Zero);

            await scenario(_ =>
            {
                _.Get.Url("/with/dateoffset/" + time.ToString("o"));
                _.StatusCodeShouldBeOk();
            });

            RoutedEndpoint.LastTimeOffset.ShouldBe(time);
        }

        [Fact]
        public Task use_integer_route_argument()
        {
            return scenario(_ =>
            {
                _.Get.Url("/with/number/55");
                _.ContentShouldBe("55");
            });
        }
    }

    public class RoutedEndpoint
    {
        public static DateTimeOffset LastTimeOffset { get; set; }

        public static DateTime LastTime { get; set; }

        public string get_with_number_value(int value)
        {
            return value.ToString();
        }

        public void get_with_date_time(DateTime time)
        {
            LastTime = time;
        }

        public void get_with_dateoffset_time(DateTimeOffset time)
        {
            LastTimeOffset = time;
        }
    }
}
