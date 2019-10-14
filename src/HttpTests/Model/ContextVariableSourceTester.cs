using System.Security.Claims;
using System.Threading;
using JasperHttp.Model;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace HttpTests.Model
{
    public class ContextVariableSourceTester
    {
        private readonly ContextVariableSource theSource = new ContextVariableSource();

        private void matchesAndCanCreate<T>()
        {
            theSource.Matches(typeof(T)).ShouldBeTrue();
            theSource.Create(typeof(T)).VariableType.ShouldBe(typeof(T));
        }

        [Fact]
        public void can_find_authentication_manager()
        {
            matchesAndCanCreate<AuthenticationManager>();
        }

        [Fact]
        public void can_find_cancellation_token()
        {
            matchesAndCanCreate<CancellationToken>();
        }

        [Fact]
        public void can_find_principal()
        {
            matchesAndCanCreate<ClaimsPrincipal>();
        }

        [Fact]
        public void can_find_request()
        {
            matchesAndCanCreate<HttpRequest>();
        }

        [Fact]
        public void can_find_response()
        {
            matchesAndCanCreate<HttpResponse>();
        }
    }
}
