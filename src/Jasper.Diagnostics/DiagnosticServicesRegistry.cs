using System.Reflection;
using Jasper.Remotes.Messaging;
using StructureMap;

namespace Jasper.Diagnostics
{
    public class DiagnosticServicesRegistry : Registry
    {
        public DiagnosticServicesRegistry()
        {
            var assembly = typeof(IDiagnosticsClient).GetTypeInfo().Assembly;

            JsonSerialization.RegisterTypesFrom(assembly);

            ForSingletonOf<ISocketConnectionManager>().Use<SocketConnectionManager>();
            ForSingletonOf<IEventAggregator>().Use<EventAggregator>();
            ForSingletonOf<IDiagnosticsClient>().Use<DiagnosticsClient>();
            ForSingletonOf<IMessagingHub>().Use<MessagingHub>();

            Scan(_ =>
            {
                _.AssemblyContainingType<IDiagnosticsClient>();
                _.AddAllTypesOf<IListener>();
            });
        }
    }
}
