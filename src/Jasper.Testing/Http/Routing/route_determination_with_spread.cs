using System.Linq;
using Baseline;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.Routing
{
    public class route_determination_with_spread
    {
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
        public void generate_code_with_spread_smoke_test()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Http.Actions.IncludeType<SpreadHttpActions>();


                _.Handlers.DisableConventionalDiscovery();

            }))
            {
                var code = runtime.Get<RouteGraph>()
                    .ChainForAction<SpreadHttpActions>(x => x.get_file(null))
                    .SourceCode;

                code.ShouldNotBeNull();
            }
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
