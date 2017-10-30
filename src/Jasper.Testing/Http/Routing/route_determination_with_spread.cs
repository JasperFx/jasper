using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using AlbaForJasper;
using Baseline;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.Routing
{
    public class route_determination_with_spread : IDisposable
    {
        private readonly JasperRuntime _runtime;

        public route_determination_with_spread()
        {
            _runtime = JasperRuntime.For(_ =>
            {
                _.Http.Actions.IncludeType<SpreadHttpActions>();
                _.Http.Actions.DisableConventionalDiscovery();

                _.Handlers.DisableConventionalDiscovery();

            });
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }

        [Fact]
        public void route_with_relative_path()
        {
            var route = RouteBuilder.Build<SpreadHttpActions>(x => x.get_folder(null));
            route.Pattern.ShouldBe("folder/...");
            route.Segments.Last().ShouldBeOfType<Spread>();
        }

        [Fact]
        public void route_with_path_segments()
        {
            var route = RouteBuilder.Build<SpreadHttpActions>(x => x.get_file(null));
            route.Pattern.ShouldBe("file/...");
            route.Segments.Last().ShouldBeOfType<Spread>();
        }


        [Fact]
        public async Task end_to_end_with_relative_path()
        {
            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/folder/a/b/c");
                _.ContentShouldBe("a/b/c");
            });

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/folder/a/b/c/123");
                _.ContentShouldBe("a/b/c/123");
            });
        }

        [Fact]
        public async Task end_to_end_with_path_segments()
        {
            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/file/abc.txt");
                _.ContentShouldBe("abc.txt");
            });

            await _runtime.Scenario(_ =>
            {
                _.Get.Url("/file/1/2/3/abc.txt");
                _.ContentShouldBe("1-2-3-abc.txt");
            });
        }
    }

    public class SpreadHttpActions
    {
        public string get_folder(string relativePath)
        {
            return relativePath;
        }

        public string get_file(string[] pathSegments)
        {
            return pathSegments.Join("-");
        }
    }
}
