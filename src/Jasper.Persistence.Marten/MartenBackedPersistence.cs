using System;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Persistence.Marten.Persistence.Sagas;
using Jasper.Persistence.Postgresql;
using LamarCodeGeneration.Model;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Marten
{
    /// <summary>
    ///     Opts into using Marten as the backing message store
    /// </summary>
    public class MartenBackedPersistence : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {


            options.Services.AddTransient<IEnvelopePersistence, PostgresqlEnvelopePersistence>();
            options.Settings.Alter<StoreOptions>(options =>
            {
                options.Schema.For<ErrorReport>().Duplicate(x => x.MessageType).Duplicate(x => x.ExceptionType);
            });

            options.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

            var frameProvider = new MartenSagaPersistenceFrameProvider();
            options.CodeGeneration.SetSagaPersistence(frameProvider);
            options.CodeGeneration.SetTransactions(frameProvider);
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
            return Variable.For<PostgresqlEnvelopePersistence>();
        }
    }
}
