using Microsoft.AspNetCore.Http.Features.Authentication;

namespace JasperHttpTesting.Authentication
{
    public interface IForwardingAuthenticationHandler : IAuthenticationHandler
    {
        IAuthenticationHandler PriorHandler { get; set; }
    }
}