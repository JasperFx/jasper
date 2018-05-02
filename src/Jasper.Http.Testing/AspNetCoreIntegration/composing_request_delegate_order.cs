using System;
using System.Threading.Tasks;
using Alba;
using Jasper.Testing;
using JasperHttpTesting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Http.Testing.AspNetCoreIntegration
{
    public class composing_request_delegate_order : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private JasperRuntime _runtime;



        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private async Task<IScenarioResult> scenario(Action<Scenario> configure)
        {
            theRegistry.Handlers.DisableConventionalDiscovery(true);

            if (_runtime == null)
            {
                _runtime = await JasperRuntime.ForAsync(theRegistry);
            }


            return await _runtime.Scenario(configure);
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
        public Task use_middleware_in_front()
        {
            theRegistry.Hosting.Configure(app =>
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

        [Fact]
        public Task default_404_behavior_with_middleware_in_front()
        {
            theRegistry.Hosting.Configure(app =>
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
        public async Task use_middleware_behind_jasper()
        {
            theRegistry.Hosting.Configure(app =>
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
        public async Task put_jasper_in_the_middle()
        {
            theRegistry.Hosting.Configure(app =>
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
