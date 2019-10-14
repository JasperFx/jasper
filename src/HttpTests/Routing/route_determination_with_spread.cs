using System.Linq;
using System.Threading.Tasks;
using Alba;
using Baseline;
using JasperHttp.Routing;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class route_determination_with_spread : RegistryContext<RoutingApp>
    {
        public route_determination_with_spread(RegistryFixture<RoutingApp> fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task end_to_end_with_path_segments()
        {
            await scenario(_ =>
            {
                _.Get.Url("/file/abc.txt");
                _.ContentShouldBe("abc.txt");
            });

            await scenario(_ =>
            {
                _.Get.Url("/file/1/2/3/abc.txt");
                _.ContentShouldBe("1-2-3-abc.txt");
            });
        }


        [Fact]
        public async Task end_to_end_with_relative_path()
        {
            await scenario(_ =>
            {
                _.Get.Url("/folder/a/b/c");
                _.ContentShouldBe("a/b/c");
            });

            await scenario(_ =>
            {
                _.Get.Url("/folder/a/b/c/123");
                _.ContentShouldBe("a/b/c/123");
            });
        }

        [Fact]
        public void route_with_path_segments()
        {
            var route = RouteBuilder.Build<SpreadHttpActions>(x => x.get_file(null));
            route.Pattern.ShouldBe("file/...");
            route.Segments.Last().ShouldBeOfType<Spread>();
        }

        [Fact]
        public void route_with_relative_path()
        {
            var route = RouteBuilder.Build<SpreadHttpActions>(x => x.get_folder(null));
            route.Pattern.ShouldBe("folder/...");
            route.Segments.Last().ShouldBeOfType<Spread>();
        }
    }


    public class SpreadHttpActions
    {
        // SAMPLE: SpreadHttpActions-by-path
        // Responds to "GET: /folder/..."
        public string get_folder(string relativePath)
        {
            return relativePath;
        }
        // ENDSAMPLE

        // SAMPLE: SpreadHttpActions-by-segments
        public string get_file(string[] pathSegments)
        {
            return pathSegments.Join("-");
        }

        // ENDSAMPLE
    }
}
