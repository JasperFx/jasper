using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace JasperHttp.Tests.Compilation
{
    public class using_context_and_context_variables : CompilationContext<ContextUsingHandler>
    {

        [Fact]
        public async Task use_HttpContext_in_method_action()
        {
            await Execute(x => x.One(null));

            ContextUsingHandler.LastContext.ShouldBeSameAs(theContext);
        }

        [Fact]
        public async Task use_HttpRequest_in_method_action()
        {
            await Execute(x => x.Two(null));

            ContextUsingHandler.LastRequest.ShouldBeSameAs(theContext.Request);
        }

        [Fact]
        public async Task use_HttpResponse_in_method_action()
        {
            await Execute(x => x.Three(null));

            ContextUsingHandler.LastResponse.ShouldBeSameAs(theContext.Response);
        }

        [Fact]
        public async Task use_ClaimsPrincipal_in_method_action()
        {
            var principal = new ClaimsPrincipal();
            theContext.User = principal;

            await Execute(x => x.Four(null));

            ContextUsingHandler.LastPrincipal.ShouldBeSameAs(principal);
        }


    }

    public class ContextUsingHandler
    {
        public void One(HttpContext context)
        {
            LastContext = context;
        }

        public void Two(HttpRequest request)
        {
            LastRequest = request;
        }

        public void Three(HttpResponse response)
        {
            LastResponse = response;
        }

        public void Four(ClaimsPrincipal principal)
        {
            LastPrincipal = principal;
        }

        public static ClaimsPrincipal LastPrincipal { get; set; }

        public static HttpContext LastContext { get; set; }
        public static HttpRequest LastRequest { get; set; }
        public static HttpResponse LastResponse { get; set; }
    }




}