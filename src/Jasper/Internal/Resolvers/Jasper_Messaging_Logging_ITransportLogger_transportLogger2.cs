using Lamar.IoC;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_Messaging_Logging_ITransportLogger_transportLogger2
    public class Jasper_Messaging_Logging_ITransportLogger_transportLogger2 : Lamar.IoC.Resolvers.SingletonResolver<Jasper.Messaging.Logging.ITransportLogger>
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory113304827;
        private readonly Lamar.IoC.Scope _topLevelScope;

        public Jasper_Messaging_Logging_ITransportLogger_transportLogger2([Lamar.Named("loggerFactory2")] Microsoft.Extensions.Logging.ILoggerFactory loggerFactory113304827, Lamar.IoC.Scope topLevelScope) : base(topLevelScope)
        {
            _loggerFactory113304827 = loggerFactory113304827;
            _topLevelScope = topLevelScope;
        }



        public override Jasper.Messaging.Logging.ITransportLogger Build(Lamar.IoC.Scope scope)
        {
            var nulloMetrics = new Jasper.Messaging.Logging.NulloMetrics();
            return new Jasper.Messaging.Logging.TransportLogger(_loggerFactory113304827, nulloMetrics);
        }

    }

    // END: Jasper_Messaging_Logging_ITransportLogger_transportLogger2
    
    
}

