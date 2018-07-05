using Lamar.IoC;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_Messaging_Logging_IMessageLogger_messageLogger
    public class Jasper_Messaging_Logging_IMessageLogger_messageLogger : Lamar.IoC.Resolvers.SingletonResolver<Jasper.Messaging.Logging.IMessageLogger>
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory_684399662;
        private readonly Lamar.IoC.Scope _topLevelScope;

        public Jasper_Messaging_Logging_IMessageLogger_messageLogger([Lamar.Named("loggerFactory2")] Microsoft.Extensions.Logging.ILoggerFactory loggerFactory_684399662, Lamar.IoC.Scope topLevelScope) : base(topLevelScope)
        {
            _loggerFactory_684399662 = loggerFactory_684399662;
            _topLevelScope = topLevelScope;
        }



        public override Jasper.Messaging.Logging.IMessageLogger Build(Lamar.IoC.Scope scope)
        {
            var nulloMetrics = new Jasper.Messaging.Logging.NulloMetrics();
            return new Jasper.Messaging.Logging.MessageLogger(_loggerFactory_684399662, nulloMetrics);
        }

    }

    // END: Jasper_Messaging_Logging_IMessageLogger_messageLogger
    
    
}

