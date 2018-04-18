using System;
using Jasper.Configuration;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.DbObjects;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Persistence.Sagas;
using Jasper.Messaging.Persistence;
using Jasper.Messaging.Transports;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Marten
{

    /// <summary>
    /// Opts into using Marten as the backing message store
    /// </summary>
    public class MartenBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.AddTransient<IEnvelopePersistor, MartenEnvelopePersistor>();
            registry.Services.AddSingleton<IPersistence, MartenBackedMessagePersistence>();
            registry.Settings.Alter<StoreOptions>(options =>
            {
                options.Storage.Add<PostgresqlEnvelopeStorage>();
            });

            registry.Services.AddSingleton<IHostedService, SchedulingAgent>();

            registry.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

            registry.Handlers.PersistSagasWith(new MartenSagaPersistence());
        }
    }

    internal static class MethodVariablesExtensions
    {
        internal static bool IsUsingMartenPersistence(this IMethodVariables method)
        {
            return method.TryFindVariable(typeof(MartenBackedPersistenceMarker), VariableSource.NotServices) != null;
        }
    }

    internal class MartenBackedPersistenceMarker : IVariableSource
    {
        public bool Matches(Type type)
        {
            return type == GetType();
        }

        public Variable Create(Type type)
        {
            return Variable.For<MartenBackedMessagePersistence>();
        }
    }
}
