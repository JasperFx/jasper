using System;
using System.Threading.Tasks;
using Alba;
using Jasper;
using JasperHttp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TestingSupport;
using Xunit;

namespace HttpTests.AspNetCoreIntegration
{
    public class EmptyStartup
    {
        public void Configure()
        {
        }
    }

    public class composing_request_delegate_order_within_aspnetcore_bootstrapping : IDisposable
    {
        public void Dispose()
        {
            _alba?.Dispose();
        }

        private SystemUnderTest _alba;

        private void theAppIs(Action<IApplicationBuilder> configure)
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();

            _alba = SystemUnderTest.For(builder =>
            {
                builder
                    .UseServer(new NulloServer())
                    .UseStartup<EmptyStartup>()
                    .Configure(configure)
                    .UseJasper(registry);
            });
        }

        private Task<IScenarioResult> scenario(Action<Scenario> configure)
        {
            if (_alba == null) theAppIs(_ => { });

            return _alba.Scenario(configure);
        }

        [Fact]
        public Task default_404_behavior()
        {
            return scenario(_ =>
            {
                _.Get.Url("/wacky");
                _.StatusCodeShouldBe(404);
                _.ContentShouldBe("Resource Not Found");
            });
        }

        [Fact]
        public Task default_404_behavior_with_middleware_in_front()
        {
            theAppIs(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });
            });


            return scenario(_ =>
            {
                _.Get.Url("/wacky");
                _.StatusCodeShouldBe(404);
                _.ContentShouldBe("Resource Not Found");

                _.Header("x-middleware").SingleValueShouldEqual("true");
            });
        }

        [Fact]
        public async Task put_jasper_in_the_middle()
        {
            theAppIs(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });

                app.UseJasper();

                app.Run(c =>
                {
                    c.Response.StatusCode = 200;
                    c.Response.ContentType = "text/plain";
                    return c.Response.WriteAsync("middleware behind was called");
                });
            });

            // Hit Jasper itself
            await scenario(_ =>
            {
                _.Get.Url("/jasper/trace");
                _.ContentShouldBe("jasper was called");
                _.ContentTypeShouldBe("text/plain");
                _.Header("x-middleware").SingleValueShouldEqual("true");
            });

            // Pass through Jasper
            await scenario(_ =>
            {
                _.Get.Url("/something/jasper/does/not/know");
                _.ContentShouldBe("middleware behind was called");
                _.Header("x-middleware").SingleValueShouldEqual("true");
            });
        }

        [Fact]
        public async Task use_jasper_http_handlers_by_default()
        {
            await scenario(_ =>
            {
                _.Get.Url("/jasper/trace");
                _.ContentShouldBe("jasper was called");
                _.ContentTypeShouldBe("text/plain");
            });
        }

        [Fact]
        public async Task use_middleware_behind_jasper()
        {
            theAppIs(app =>
            {
                app.UseJasper();

                app.Run(c =>
                {
                    c.Response.StatusCode = 200;
                    c.Response.ContentType = "text/plain";
                    return c.Response.WriteAsync("middleware behind was called");
                });
            });

            await scenario(_ =>
            {
                _.Get.Url("/jasper/trace");
                _.ContentShouldBe("jasper was called");
                _.ContentTypeShouldBe("text/plain");
            });

            await scenario(_ =>
            {
                _.Get.Url("/something/jasper/does/not/know");
                _.ContentShouldBe("middleware behind was called");
            });
        }

        [Fact]
        public Task use_middleware_in_front()
        {
            theAppIs(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });
            });

            return scenario(_ =>
            {
                _.Get.Url("/jasper/trace");
                _.ContentShouldBe("jasper was called");
                _.ContentTypeShouldBe("text/plain");
                _.Header("x-middleware").SingleValueShouldEqual("true");
            });
        }
    }
}
