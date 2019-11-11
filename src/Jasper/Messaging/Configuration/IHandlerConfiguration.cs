using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using LamarCodeGeneration;

namespace Jasper.Messaging.Configuration
{
    public interface IHandlerConfiguration : IHasRetryPolicies
    {
        /// <summary>
        ///     Configure how Jasper discovers message handler classes to override
        ///     the built in conventions
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        IHandlerConfiguration Discovery(Action<HandlerSource> configure);


        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void GlobalPolicy<T>() where T : IHandlerPolicy, new();

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        void GlobalPolicy(IHandlerPolicy policy);

    }


}
