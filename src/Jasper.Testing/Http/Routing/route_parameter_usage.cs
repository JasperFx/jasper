using System;
using System.Threading.Tasks;
using Alba;
using Baseline.Dates;
using Baseline.Reflection;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Testing.Http.ContentHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.Routing
{
    public class route_parameter_usage
    {
        [Fact]
        public void methods_are_candidate_actions()
        {
            ActionSource.IsCandidate(ReflectionHelper.GetMethod<RoutedEndpoint>(x => x.get_with_date_time(DateTime.MinValue))).ShouldBeTrue();
            ActionSource.IsCandidate(ReflectionHelper.GetMethod<RoutedEndpoint>(x => x.get_with_dateoffset_time(DateTimeOffset.MaxValue))).ShouldBeTrue();
            ActionSource.IsCandidate(ReflectionHelper.GetMethod<RoutedEndpoint>(x => x.get_with_number_value(55))).ShouldBeTrue();
        }

        [Fact]
        public async Task use_date_time_route_arguments()
        {
            RoutedEndpoint.LastTime = DateTime.MinValue;
            var time = DateTime.Today.AddHours(3);

            await HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/with/date/" + time.ToString("o"));
                _.StatusCodeShouldBeOk();
            });

            RoutedEndpoint.LastTime.ShouldBe(time);


        }

        [Fact]
        public async Task use_date_timeoffset_route_arguments()
        {
            RoutedEndpoint.LastTimeOffset = DateTimeOffset.MinValue;;
            var time = new DateTimeOffset(2017, 8, 15, 8, 0, 0, 0, TimeSpan.Zero);

            await HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/with/dateoffset/" + time.ToString("o"));
                _.StatusCodeShouldBeOk();
            });

            RoutedEndpoint.LastTimeOffset.ShouldBe(time);


        }

        [Fact]
        public Task use_integer_route_argument()
        {
            var route = RouteBuilder.Build<RoutedEndpoint>(x => x.get_with_number_value(55));

            var routes = HttpTesting.Runtime.Get<RouteGraph>();


            return HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/with/number/55");
                _.ContentShouldBe("55");
            });
        }
    }

    public class RoutedEndpoint
    {
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

        public static DateTimeOffset LastTimeOffset { get; set; }

        public static DateTime LastTime { get; set; }
    }
}
