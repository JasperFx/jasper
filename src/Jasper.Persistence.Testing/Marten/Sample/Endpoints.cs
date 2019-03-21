using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Testing.Marten.Sample
{
    // SAMPLE: MartenUsingEndpoint-with-ctor-injection
    public class MartenUsingEndpoint
    {
        private readonly IQuerySession _session;
        private readonly ILogger<User> _logger;

        public MartenUsingEndpoint(IQuerySession session, ILogger<User> logger)
        {
            _session = session;
            _logger = logger;
        }

        public Task<User> get_user_id(string id)
        {
            _logger.LogDebug("I loaded a user");
            return _session.LoadAsync<User>(id);
        }
    }
    // ENDSAMPLE

    // SAMPLE: MartenStaticEndpoint
    public static class MartenStaticEndpoint
    {
        public static Task<User> get_user_id(
            string id,

            // Gets passed in by Jasper at runtime
            IQuerySession session,

            // Gets passed in by Jasper at runtime
            ILogger<User> logger)
        {
            logger.LogDebug("I loaded a user");
            return session.LoadAsync<User>(id);
        }
    }
    // ENDSAMPLE
}
