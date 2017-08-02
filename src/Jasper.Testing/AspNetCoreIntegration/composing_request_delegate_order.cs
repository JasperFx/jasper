using System;
using System.Threading.Tasks;
using Alba;
using AlbaForJasper;
using Jasper.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Testing.AspNetCoreIntegration
{
    public class composing_request_delegate_order : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly Lazy<JasperRuntime> _runtime;

        public composing_request_delegate_order()
        {
            theRegistry.Messages.Handlers.ConventionalDiscoveryDisabled = true;
            _runtime = new Lazy<JasperRuntime>(() => JasperRuntime.For(theRegistry));
        }

        public void Dispose()
        {
            if (_runtime.IsValueCreated)
            {
                _runtime.Value.Dispose();
            }
        }

        private Task<IScenarioResult> scenario(Action<Scenario> configure)
        {
            return _runtime.Value.Scenario(configure);
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
            theRegistry.Http.Configure(app =>
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
            theRegistry.Http.Configure(app =>
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
            theRegistry.Http.Configure(app =>
            {
                app.AddJasper();

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
            theRegistry.Http.Configure(app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        context.Response.Headers["x-middleware"] = "true";

                        return next(context);
                    };
                });

                app.AddJasper();

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
