using System;
using System.IO;
using System.Reflection;
using Jasper.Messaging;
using Lamar;

namespace Jasper
{
    /// <summary>
    /// Represents the runtime of a Jasper application. Encapsulates the ASP.net Core IWebHost
    /// </summary>
    public interface IJasperHost : IDisposable
    {
        /// <summary>
        ///     The main application assembly for the running application
        /// </summary>
        Assembly ApplicationAssembly { get; }

        /// <summary>
        ///     The underlying Lamar container
        /// </summary>
        IContainer Container { get; }

        string[] HttpAddresses { get; }

        /// <summary>
        ///     Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        IMessagePublisher Messaging { get; }

        /// <summary>
        ///     The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        string ServiceName { get; }

        bool IsDisposed { get; }

        /// <summary>
        ///     Writes a textual report about the configured transports and servers
        ///     for this application
        /// </summary>
        /// <param name="writer"></param>
        void Describe(TextWriter writer);

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>();

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Get(Type type);
    }
}
