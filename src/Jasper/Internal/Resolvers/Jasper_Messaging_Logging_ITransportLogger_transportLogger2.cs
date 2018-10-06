using Lamar.IoC;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_Messaging_Logging_ITransportLogger_transportLogger2
    public class Jasper_Messaging_Logging_ITransportLogger_transportLogger2 : Lamar.IoC.Resolvers.SingletonResolver<Jasper.Messaging.Logging.ITransportLogger>
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory_339349292;
        private readonly Lamar.IoC.Scope _topLevelScope;

        public Jasper_Messaging_Logging_ITransportLogger_transportLogger2([Lamar.Named("loggerFactory2")] Microsoft.Extensions.Logging.ILoggerFactory loggerFactory_339349292, Lamar.IoC.Scope topLevelScope) : base(topLevelScope)
        {
            _loggerFactory_339349292 = loggerFactory_339349292;
            _topLevelScope = topLevelScope;
        }



        public override Jasper.Messaging.Logging.ITransportLogger Build(Lamar.IoC.Scope scope)
        {
            var nulloMetrics = new Jasper.Messaging.Logging.NulloMetrics();
            return new Jasper.Messaging.Logging.TransportLogger(_loggerFactory_339349292, nulloMetrics);
        }

    }

    // END: Jasper_Messaging_Logging_ITransportLogger_transportLogger2
    
    
}

