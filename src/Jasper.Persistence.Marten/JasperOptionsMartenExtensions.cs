using System;
using Marten;

namespace Jasper.Persistence.Marten
{
    public static class JasperOptionsMartenExtensions
    {
        /// <summary>
        ///     Integrate Marten with this Jasper application. Adds basic Marten IDocumentStore,
        ///     IDocumentSession, IQuerySession, enabling Marten backed Saga persistence,
        ///     and using Postgresql for message durability using default Marten configuration
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString"></param>
        public static void UseMarten(this IExtensions extensions, string connectionString)
        {
            extensions.UseMarten(o => o.Connection(connectionString));
        }

        /// <summary>
        ///     Integrate Marten with this Jasper application. Adds basic Marten IDocumentStore,
        ///     IDocumentSession, IQuerySession, enabling Marten backed Saga persistence,
        ///     and using Postgresql for message durability. This overload allows for customizing
        ///     Marten's StoreOptions
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString"></param>
        public static void UseMarten(this IExtensions extensions, Action<StoreOptions> configureMarten)
        {
            extensions.Include<MartenExtension>(x => configureMarten(x.Options));
        }
    }
}
