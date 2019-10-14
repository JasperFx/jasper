using System;
using System.Threading.Tasks;
using Alba;
using Jasper;
using Jasper.TestSupport.Alba;
using Microsoft.AspNetCore.Http;
using TestingSupport;
using Xunit;

namespace HttpTests.AspNetCoreIntegration
{
    public class composing_request_delegate_order : IDisposable
    {
        public void Dispose()
        {
            _host?.Dispose();
        }

        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private SystemUnderTest _host;

        private async Task<IScenarioResult> scenario(Action<Scenario> configure)
        {
            theRegistry.Handlers.DisableConventionalDiscovery();

            if (_host == null) _host = JasperAlba.For(theRegistry);

            return await _host.Scenario(configure);
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
            theRegistry.Hosting(x => x.Configure(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });
            }));

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
            theRegistry.Hosting(x => x.Configure(app =>
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
            }));

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
            theRegistry.Hosting(x => x.Configure(app =>
            {
                app.UseJasper();

                app.Run(c =>
                {
                    c.Response.StatusCode = 200;
                    c.Response.ContentType = "text/plain";
                    return c.Response.WriteAsync("middleware behind was called");
                });
            }));

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
            theRegistry.Hosting(x => x.Configure(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });
            }));

            return scenario(_ =>
            {
                _.Get.Url("/jasper/trace");
                _.ContentShouldBe("jasper was called");
                _.ContentTypeShouldBe("text/plain");
                _.Header("x-middleware").SingleValueShouldEqual("true");
            });
        }
    }

    public class TracingEndpoint
    {
        public static Task get_jasper_trace(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("jasper was called");
        }
    }
}
