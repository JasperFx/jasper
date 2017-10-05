using Jasper.Remotes.Messaging;
using Jasper.WebSockets;
using Newtonsoft.Json.Serialization;

namespace Jasper.Diagnostics
{
    public interface IDiagnosticsClient
    {
        void Send<T>(T message);
    }

    public class DiagnosticsClient : IDiagnosticsClient
    {
        private readonly ISocketConnectionManager _manager;
        private IContractResolver _contractResolver;

        public DiagnosticsClient(ISocketConnectionManager manager)
        {
            _manager = manager;
            _contractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public void Send<T>(T message)
        {
            var json = JsonSerialization.ToCleanJson(message, true, _contractResolver);
            _manager.SendToAllAsync(json).ConfigureAwait(false);
        }
    }
}
