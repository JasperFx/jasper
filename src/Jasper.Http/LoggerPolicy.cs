using System;
using System.Linq;
using Baseline;
using BlueMilk;
using BlueMilk.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Http
{
    internal class LoggerPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (!type.Closes(typeof(ILogger<>)))
            {
                return null;
            }

            var inner = type.GetGenericArguments().Single();
            var loggerType = typeof(Logger<>).MakeGenericType(inner);

            Instance instance = null;
            if (inner.IsPublic)
            {
                instance = new ConstructorInstance(type, loggerType,
                    ServiceLifetime.Transient);
            }
            else
            {
                instance = new LambdaInstance(type, provider =>
                {
                    var factory = provider.GetService<ILoggerFactory>();
                    return Activator.CreateInstance(loggerType, factory);

                }, ServiceLifetime.Transient);
            }

            return new ServiceFamily(type, instance);
        }
    }
}
