using System.Security.Claims;
using JasperHttpTesting.Authentication;

namespace JasperHttpTesting
{
    public static class ScenarioExtensions
    {
        // SAMPLE: with-windows-authentication-extension
        public static Scenario WithWindowsAuthentication(this Scenario scenario, ClaimsPrincipal user = null)
        {
            scenario.Context.AttachAuthenticationHandler(new StubWindowsAuthHandler(scenario.Context), user);
            return scenario;
        }
        // ENDSAMPLE
    }
}
